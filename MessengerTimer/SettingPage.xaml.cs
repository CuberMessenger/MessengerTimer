using MessengerTimer.DataModels;
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

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page {
        //Timing Scramble Statistics UI
        public IEnumerable<SettingItemGroup> Groups { get; set; }

        public SettingPage() {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            Groups = SettingItemGroup.Instance;
        }

        private void ListView_GotFocus(object sender, RoutedEventArgs e) => SettingSemanticZoom.StartBringIntoView();

        private void ComboBox_Loaded(object sender, RoutedEventArgs e) {
            try {
                var s = sender as ComboBox;
                var i = s.DataContext as SettingItem;
                s.SelectedItem = s.Items[i.ComboBoxSelectedIndex];
            }
            catch (NullReferenceException NRE) {
                Console.WriteLine(NRE.Message);
            }
            catch (IndexOutOfRangeException) {
                ((sender as ComboBox).DataContext as SettingItem).ComboBoxSelectedIndex = 0;
                ComboBox_Loaded(sender, e);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var s = sender as ComboBox;
            var i = s.DataContext as SettingItem;
            i.ComboBoxSelectedIndex = s.SelectedIndex;
            if (i.Title == "TimerFormat: ") {
                App.MainPageInstance.RefreshAoNResults();
                App.MainPageInstance.ResetTimer();
            }
        }
    }
}
