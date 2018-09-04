using MessengerTimer.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ResultPage : Page {
        private static AppSettings appSettings = App.MainPageInstance.appSettings;

        public ObservableCollection<Result> Results = MainPage.Results;

        public ObservableCollection<ResultGroup> ResultGroups = MainPage.allResult.ResultGroups;

        private bool NeedReload = true;

        public ResultPage() {
            this.InitializeComponent();
            this.Loaded += ResultPage_Loaded;
        }

        private void ResultPage_Loaded(object sender, RoutedEventArgs e) => UpdateUI();

        private void RefreshMainPageDotResults() {
            MainPage.Results.Clear();
            for (int i = 0; i < ResultGroups[appSettings.CurrentDataGroupIndex].Results.Count; i++)
                MainPage.Results.Add(ResultGroups[appSettings.CurrentDataGroupIndex].Results[i]);
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var cg = (sender as ComboBox).SelectedItem;
            appSettings.CurrentDataGroupIndex = Math.Max(0, ResultGroups.IndexOf(cg as ResultGroup));
            if (appSettings.CurrentDataGroupIndex >= 0) {
                RefreshMainPageDotResults();
                App.MainPageInstance.ChangeScrambleType();
                App.MainPageInstance.RefreshAoNResults();
            }

            if (NeedReload)
                App.MainPageInstance.ReloadInfoFramePage(typeof(ResultPage));
            NeedReload = true;
        }

        private async void ShowAlertDialogAsync(string message) {
            ContentDialog contentDialog = new ContentDialog { Title = message, CloseButtonText = "OK" };
            await contentDialog.ShowAsync();
        }

        private void ConfirmAddDataGroupButton_Click(object sender, RoutedEventArgs e) {
            string groupName = NewDataGroupNameTextBox.Text;
            if (!String.IsNullOrWhiteSpace(groupName)) {
                ResultGroups.Add(new ResultGroup { Results = new ObservableCollection<Result>(), GroupName = groupName });
                appSettings.CurrentDataGroupIndex = ResultGroups.Count - 1;
                RefreshMainPageDotResults();
                MainPage.SaveDataAsync(false);

                GroupComboBox.SelectedIndex = GroupComboBox.Items.Count - 1;

                ShowAlertDialogAsync($"{groupName} added!");

                if (NeedReload)
                    App.MainPageInstance.ReloadInfoFramePage(typeof(ResultPage));
            }
            else
                ShowAlertDialogAsync("Invalid DataGroup Name!");

            NewDataGroupNameTextBox.Text = String.Empty;
            AddDataGroupButton.Flyout.Hide();
        }

        private void ConfirmDeleteCurrentDataGroupButton_Click(object sender, RoutedEventArgs e) {
            if (MainPage.allResult.ResultGroups.Count <= 1) {
                ShowAlertDialogAsync("Cannot delete all groups!");
                goto End;
            }
            //1. Delete content from memory
            MainPage.allResult.ResultGroups.RemoveAt(appSettings.CurrentDataGroupIndex);

            //2. Delete content from disk
            MainPage.SaveDataAsync(true);

            //3. Remove ComboBox item
            MainPage.Results.Clear();

            //4. Select another default content
            GroupComboBox.SelectedIndex = -1;

            ShowAlertDialogAsync("Results Deleted!");

            End:
            DeleteCurrentDataGroupButton.Flyout.Hide();
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) {
            MenuFlyout menuFlyout = new MenuFlyout();

            var currentResult = (sender as FrameworkElement).DataContext;
            int index = MainPage.Results.IndexOf(currentResult as Result);
            var modifyFlyoutItem = new MenuFlyoutItem { Text = "Modify", Tag = index };
            var deleteFlyoutItem = new MenuFlyoutItem { Text = "Delete", Tag = index };
            var copyScrambleFlyoutItem = new MenuFlyoutItem { Text = "Copy Scramble", Tag = index };

            modifyFlyoutItem.Click += ModifyFlyoutItem_Click;
            deleteFlyoutItem.Click += DeleteFlyoutItem_Click;
            copyScrambleFlyoutItem.Click += CopyScrambleFlyoutItem_Click;

            menuFlyout.Items.Add(modifyFlyoutItem);
            menuFlyout.Items.Add(deleteFlyoutItem);
            menuFlyout.Items.Add(copyScrambleFlyoutItem);

            menuFlyout.ShowAt(sender as FrameworkElement);
            GC.Collect();
        }

        private void CopyScrambleFlyoutItem_Click(object sender, RoutedEventArgs e) {
            var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            dataPackage.SetText(Results[(int)(sender as MenuFlyoutItem).Tag].Scramble);
            Clipboard.SetContent(dataPackage);
        }

        private void DeleteFlyoutItem_Click(object sender, RoutedEventArgs e) {
            App.MainPageInstance.DeleteResult((int)(sender as MenuFlyoutItem).Tag);
            UpdateTotalStatistics();
        }

        private void SetCheckedRadioButton(Punishment punishment) => RadioButtonsStackPanel.Children.OfType<RadioButton>().ToList()[(int)punishment].IsChecked = true;

        private Punishment GetCheckedPunishment() {
            var radioButtons = RadioButtonsStackPanel.Children.OfType<RadioButton>().ToList();
            return (Punishment)radioButtons.IndexOf(radioButtons.First(rb => (bool)rb.IsChecked));
        }

        private async void ModifyFlyoutItem_Click(object sender, RoutedEventArgs e) {
            var indexToModify = (int)(sender as MenuFlyoutItem).Tag;

            EditTextBox.Text = Results[indexToModify].ResultValue.ToString();
            SetCheckedRadioButton(Results[indexToModify].ResultPunishment);

            var dialogResult = await EditDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary) {
                var result = Double.TryParse(EditTextBox.Text, out double value);

                if (result && value > 0) {
                    //1. Modify result in current memory
                    //2. Modify result in MainPage memory Done by one line of code
                    MainPage.Results[indexToModify].ResultValue = value;
                    MainPage.Results[indexToModify].ResultPunishment = GetCheckedPunishment();

                    //3. Recalculate Ao5/Ao12 results
                    App.MainPageInstance.RefreshListOfResult(indexToModify, MainPage.Results);

                    //4. Modify result in disk
                    MainPage.SaveDataAsync(false);
                }
                else
                    ShowAlertDialogAsync("Input Format Error!");

                UpdateTotalStatistics();
            }
        }

        private void NewDataGroupNameTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter)
                ConfirmAddDataGroupButton_Click(null, null);
        }

        private async void AddResultButton_Click(object sender, RoutedEventArgs e) {
            EditTextBox.Text = String.Empty;
            NonePunishmentRadioButton.IsChecked = true;

            var dialogResult = await EditDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary) {
                var result = Double.TryParse(EditTextBox.Text, out double value);

                if (result && value > 0)
                    App.MainPageInstance.UpdateResult(new Result(
                        value,
                        MainPage.Results.Count + 2, GetCheckedPunishment(),
                        null));
                else
                    ShowAlertDialogAsync("Input Format Error!");

                UpdateTotalStatistics();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            NeedReload = false;
            GroupComboBox.SelectedIndex = appSettings.CurrentDataGroupIndex;
        }

        internal void UpdateTotalStatistics() {
            BestStringTextBlock.Text = Results.Count > 0
                ? Result.GetFormattedString(Results
                    .Where(r => r.ResultPunishment != Punishment.DNF)
                    .Min(r => r.ResultValue + (r.ResultPunishment == Punishment.PlusTwo ? 2 : 0)))
                : double.NaN.ToString();

            WorstStringTextBlock.Text = Results.Count > 0
                ? Result.GetFormattedString(Results
                    .Where(r => r.ResultPunishment != Punishment.DNF)
                    .Max(r => r.ResultValue + (r.ResultPunishment == Punishment.PlusTwo ? 2 : 0)))
                : double.NaN.ToString();

            AverageStringTextBlock.Text = Results.Count > 0
                ? Result.GetFormattedString(Results
                    .Where(r => r.ResultPunishment != Punishment.DNF)
                    .Average(r => r.ResultPunishment == Punishment.None ? r.ResultValue : r.ResultValue + 2))
                : double.NaN.ToString();
        }

        public void UpdateUI() {
            Bindings.Update();
            UpdateTotalStatistics();
        }

        private void ConfirmChangeDataGroupButton_Click(object sender, RoutedEventArgs e) {
            string groupName = ChangeDataGroupNameTextBox.Text;
            if (!String.IsNullOrWhiteSpace(groupName)) {
                MainPage.allResult.CurrentGroup().GroupName = groupName;
                MainPage.SaveDataAsync(false);
            }
            else
                ShowAlertDialogAsync("Invalid DataGroup Name!");

            ChangeDataGroupNameButton.Flyout.Hide();
        }

        private void ConfirmMergeDataGroupButton_Click(object sender, RoutedEventArgs e) {
            ResultGroup resultGroup = MergeTargetGroupComboBox.SelectedItem as ResultGroup;
            if (resultGroup == MainPage.allResult.CurrentGroup()) {
                ShowAlertDialogAsync("You need to merge two different groups!");
                goto End;
            }
            if (resultGroup is null) {
                ShowAlertDialogAsync("Please selete target group!");
                goto End;
            }
            App.MainPageInstance.MergeResultGroup(resultGroup);

            End:
            MergeDataGroupNameButton.Flyout.Hide();
        }
    }
}
