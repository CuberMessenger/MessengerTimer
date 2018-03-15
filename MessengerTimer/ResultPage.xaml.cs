using MessengerTimer.DataModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ResultPage : Page {
        public ObservableCollection<Result> Results = MainPage.Results;

        public ObservableCollection<ResultGroup> ResultGroups = MainPage.allResult.ResultGroups;

        public ResultGroup CurrentResultGroup = MainPage.allResult.ResultGroups[appSettings.CurrentDataGroupIndex];

        private static AppSettings appSettings = new AppSettings();

        public ResultPage() {
            this.InitializeComponent();
        }

        private void RefreshMainPageDotResults() {
            MainPage.Results.Clear();
            for (int i = 0; i < ResultGroups[appSettings.CurrentDataGroupIndex].Results.Count; i++)
                MainPage.Results.Add(ResultGroups[appSettings.CurrentDataGroupIndex].Results[i]);
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var cg = (sender as ComboBox).SelectedItem;
            appSettings.CurrentDataGroupIndex = ResultGroups.IndexOf(cg as ResultGroup);
            if (appSettings.CurrentDataGroupIndex >= 0) {
                RefreshMainPageDotResults();
                CurrentResultGroup = ResultGroups[appSettings.CurrentDataGroupIndex];

                ((Window.Current.Content as Frame).Content as MainPage).RefreshAoNResults();
            }
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

                CurrentResultGroup = ResultGroups[appSettings.CurrentDataGroupIndex];

                ShowAlertDialogAsync($"{groupName} added!");
            }
            else
                ShowAlertDialogAsync("Invalid DataGroup Name!");

            NewDataGroupNameTextBox.Text = String.Empty;
            AddDataGroupButton.Flyout.Hide();
        }

        private void ConfirmDeleteCurrentDataGroupButton_Click(object sender, RoutedEventArgs e) {
            //1. Delete content from memory
            MainPage.allResult.ResultGroups.RemoveAt(appSettings.CurrentDataGroupIndex);

            //2. Delete content from disk
            MainPage.SaveDataAsync(true);

            //3. Remove ComboBox item
            MainPage.Results.Clear();

            //4. Select another default content
            GroupComboBox.SelectedIndex = -1;

            ShowAlertDialogAsync("Results Deleted!");

            DeleteCurrentDataGroupButton.Flyout.Hide();
        }

        private void StackPanel_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) {
            MenuFlyout menuFlyout = new MenuFlyout();

            var currentResult = (sender as FrameworkElement).DataContext;
            MenuFlyoutItem modifyFlyoutItem = new MenuFlyoutItem { Text = "Modify", Tag = MainPage.Results.IndexOf(currentResult as Result) };
            MenuFlyoutItem deleteFlyoutItem = new MenuFlyoutItem { Text = "Delete", Tag = MainPage.Results.IndexOf(currentResult as Result) };

            modifyFlyoutItem.Click += ModifyFlyoutItem_Click;
            deleteFlyoutItem.Click += DeleteFlyoutItem_Click;

            menuFlyout.Items.Add(modifyFlyoutItem);
            menuFlyout.Items.Add(deleteFlyoutItem);

            menuFlyout.ShowAt(sender as FrameworkElement);
        }

        private void DeleteFlyoutItem_Click(object sender, RoutedEventArgs e) {
            ((Window.Current.Content as Frame).Content as MainPage).DeleteResult((int)(sender as MenuFlyoutItem).Tag);
        }

        private void SetCheckedRadioButton(Punishment punishment) {
            RadioButtonsStackPanel.Children.OfType<RadioButton>().ToList()[(int)punishment].IsChecked = true;

            //switch (punishment) {
            //    case Punishment.None:
            //        NonePunishmentRadioButton.IsChecked = true;
            //        break;
            //    case Punishment.PlusTwo:
            //        PlusTwoPunishmentRadioButton.IsChecked = true;
            //        break;
            //    case Punishment.DNF:
            //        DNFPunishmentRadioButton.IsChecked = true;
            //        break;
            //}
        }

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
                    ((Window.Current.Content as Frame).Content as MainPage).RefreshListOfResult(indexToModify);

                    //4. Modify result in disk
                    MainPage.SaveDataAsync(false);
                }
                else
                    ShowAlertDialogAsync("Input Format Error!");
            }
        }

        private void NewDataGroupNameTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                ConfirmAddDataGroupButton_Click(null, null);
            }
        }

        private async void AddResultButton_Click(object sender, RoutedEventArgs e) {
            EditTextBox.Text = String.Empty;
            NonePunishmentRadioButton.IsChecked = true;

            var dialogResult = await EditDialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary) {
                var result = Double.TryParse(EditTextBox.Text, out double value);

                if (result && value > 0)
                    ((Window.Current.Content as Frame).Content as MainPage).UpdateResult(new Result(value, MainPage.Results.Count + 2, GetCheckedPunishment()));
                else
                    ShowAlertDialogAsync("Input Format Error!");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            GroupComboBox.SelectedIndex = appSettings.CurrentDataGroupIndex;
        }
    }
}
