using MessengerTimer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MessengerTimer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ResultPage : Page
    {
        public ObservableCollection<Result> Results = MainPage.Results;

        public ObservableCollection<DataGroup> DataGroups = MainPage.DataGroups;

        public DataGroup CurrentDataGroup = DataGroup.CurrentDataGroup;

        public ResultPage()
        {
            this.InitializeComponent();

            //GroupComboBox.SelectedIndex = GroupComboBox.Items.Count > 0 ? 0 : -1;
        }

        private void RefreshMainPageDotResults()
        {
            MainPage.Results.Clear();
            for (int i = 0; i < MainPage.DataGroups[MainPage.appSettings.CurrentDataGroupIndex].Results.Count; i++)
                MainPage.Results.Add(MainPage.DataGroups[MainPage.appSettings.CurrentDataGroupIndex].Results[i]);
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cg = (sender as ComboBox).SelectedItem;
            MainPage.appSettings.CurrentDataGroupIndex = MainPage.DataGroups.IndexOf(cg as DataGroup);
            RefreshMainPageDotResults();
            DataGroup.CurrentDataGroup = MainPage.DataGroups[MainPage.appSettings.CurrentDataGroupIndex];
        }

        private bool IsValidString(string s)
        {
            return s != null && s != "";
        }

        private async void ShowAlertDialog(string message)
        {
            ContentDialog contentDialog = new ContentDialog { Title = message, CloseButtonText = "OK" };
            await contentDialog.ShowAsync();
        }

        private void ConfirmAddDataGroupButton_Click(object sender, RoutedEventArgs e)
        {
            string type = NewDataGroupNameTextBox.Text;
            if (IsValidString(type))
            {
                MainPage.DataGroups.Add(new DataGroup { Results = new ObservableCollection<Result>(), Count = 0, Type = type });
                MainPage.appSettings.CurrentDataGroupIndex = MainPage.DataGroups.Count - 1;
                RefreshMainPageDotResults();
                MainPage.SaveData();

                GroupComboBox.SelectedIndex = GroupComboBox.Items.Count - 1;

                DataGroup.CurrentDataGroup = MainPage.DataGroups[MainPage.appSettings.CurrentDataGroupIndex];

                ShowAlertDialog($"{type} added!");
            }
            else
                ShowAlertDialog("Invalid DataGroup Name!");

            AddDataGroupButton.Flyout.Hide();
        }

        private void ConfirmDeleteCurrentDataGroupButton_Click(object sender, RoutedEventArgs e)
        {


            DeleteCurrentDataGroupButton.Flyout.Hide();
        }
    }
}
