using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UIHelper;
using TimeData;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    enum DisplayMode { RealTime, OnlyOberving }

    public sealed partial class MainPage : Page {
        //Static Val
        private static Brush BlackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        //Useful Var
        private TimerStatus timerStatus { get; set; }
        private DispatcherTimer refreshTimeTimer { get; set; }
        private DispatcherTimer holdingCheckTimer { get; set; }
        private bool isHolding { get; set; }
        private List<DataGroup> dataGroups { get; set; }

        //Display Var
        private DateTime startTime { get; set; }
        private DateTime endTime { get; set; }

        //Config Var
        private bool needObserving { get; set; }
        private long startDelay { get; set; }
        private string timerFormat { get; set; }
        private DisplayMode displayMode { get; set; }

        public MainPage() {
            this.InitializeComponent();
            Init();
        }

        private void InitConfig() {
            needObserving = false;
            startDelay = 3000000;
            timerFormat = "s.fff";
            displayMode = DisplayMode.RealTime;
        }

        private void InitBingBackground() {
            var image = new ImageBrush();
            image.ImageSource = new BitmapImage(new Uri(BingImage.FetchUrlAsync(), UriKind.Absolute));
            BackGroundGrid.Background = image;
        }

        private void InitUI() {
            InitBingBackground();
            StatusTextBlock.Text = timerStatus.ToString();
            ResetTimer();
        }

        private void ParseSaveData(string raw) {
            var rawArray = raw.Split(' ');
            //rawArray: NumOfDataGroup FirstDataGroupType FirstDataGroupResultsCount Result1 Result1 Result1 ...... SecondDataGroup ...

            int index = 0;
            for (int i = 0; i < int.Parse(rawArray.First()); i++) {
                DataGroup dataGroup = new DataGroup { type = rawArray[++index], count = int.Parse(rawArray[++index]), results = new System.Collections.ObjectModel.ObservableCollection<Result>() };

                for (int j = 0; j < dataGroup.count; j++)
                    dataGroup.results.Add(new Result(dataGroup.count - j, double.Parse(rawArray[++index]), double.Parse(rawArray[++index]), double.Parse(rawArray[++index])));

                dataGroups.Add(dataGroup);
            }
        }

        private async Task ReadSaveData() {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try {
                file = await storageFolder.GetFileAsync("SaveData");
            }
            catch (FileNotFoundException) {
                await storageFolder.CreateFileAsync("SaveData");
                file = await storageFolder.GetFileAsync("SaveData");
            }

            //await FileIO.WriteTextAsync(file, "1 3x3 5 1.23 2.34 3.45 4.567 5.678 6.789 7.89 8.90 9.00 3 4 5 78 98 NaN");

            string data = await FileIO.ReadTextAsync(file);
            ParseSaveData(data);
        }

        private void FillResult(DataGroup dataGroup) {
            App.Results = dataGroup.results;

            Ao5ValueTextBlock.Text = App.Results.First().ao5Value.ToString();
            Ao12ValueTextBlock.Text = App.Results.First().ao12Value.ToString();
        }

        private async void InitResults() {
            dataGroups = new List<DataGroup>();

            await ReadSaveData();

            FillResult(dataGroups.First());
        }

        private void Init() {
            InitConfig();
            InitUI();
            InitResults();

            timerStatus = TimerStatus.Waiting;

            holdingCheckTimer = new DispatcherTimer();
            holdingCheckTimer.Interval = new TimeSpan(startDelay);
            holdingCheckTimer.Tick += HoldingCheckTimer_Tick;

            switch (displayMode) {
                case DisplayMode.RealTime:
                    refreshTimeTimer = new DispatcherTimer();
                    refreshTimeTimer.Interval = new TimeSpan(10000);
                    refreshTimeTimer.Tick += RefreshTimeTimer_Tick;

                    Window.Current.CoreWindow.KeyUp += RealTimeSpaceKeyUp;
                    Window.Current.CoreWindow.KeyDown += RealTimeSpaceKeyDown;
                    break;
                case DisplayMode.OnlyOberving:
                    //Todo
                    break;
                default:
                    break;
            }

            //Hot Key
            Window.Current.CoreWindow.KeyUp += EscapeKeyUp;
        }

        private void EscapeKeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            if (args.VirtualKey == Windows.System.VirtualKey.Escape && timerStatus == TimerStatus.Waiting)
                ResetTimer();
        }

        private void HoldingCheckTimer_Tick(object sender, object e) {
            if (isHolding) {
                timerStatus = TimerStatus.Holding;
                TimerTextBlock.Foreground = GreenBrush;
            }
            isHolding = false;
            holdingCheckTimer.Stop();
        }

        private void ResetTimer() {
            DisplayTime(new TimeSpan(0));
        }

        private void DisplayTime(TimeSpan timeSpan) {
            TimerTextBlock.Text = new DateTime(timeSpan.Ticks).ToString(timerFormat);
        }

        private async void SaveData() {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(dataGroups.Count);

            for (int i = 0; i < dataGroups.Count; i++) {
                buffer.Append(" " + dataGroups[i].type);
                buffer.Append(" " + dataGroups[i].count);

                for (int j = 0; j < dataGroups[i].count; j++)
                    buffer.Append(" " + dataGroups[i].results[j].resultValue + " " + dataGroups[i].results[j].ao5Value + " " + dataGroups[i].results[j].ao12Value);
            }

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try {
                file = await storageFolder.GetFileAsync("SaveData");
            }
            catch (FileNotFoundException) {
                await storageFolder.CreateFileAsync("SaveData");
                file = await storageFolder.GetFileAsync("SaveData");
            }

            await FileIO.WriteTextAsync(file, buffer.ToString());
        }

        private void StopTimer() {
            endTime = DateTime.Now;
            DisplayTime(endTime - startTime);
            refreshTimeTimer.Stop();

            UpdateResult(endTime - startTime);
            SaveData();
        }

        private void UpdateResult(TimeSpan result) {
            App.Results.Insert(0, new Result(result, App.Results.Count + 1));

            double ao5 = 0, ao12 = 0;

            if (App.Results.Count >= 5) {
                for (int i = 0; i < 5; i++)
                    ao5 += App.Results[i].resultValue;
                ao5 = Math.Round(ao5 / 5, 3);
            }
            else
                ao5 = double.NaN;

            if (App.Results.Count >= 12) {
                for (int i = 0; i < 12; i++)
                    ao12 += App.Results[i].resultValue;
                ao12 = Math.Round(ao12 / 12, 3);
            }
            else
                ao12 = double.NaN;

            App.Results.First().ao5Value = ao5;
            App.Results.First().ao12Value = ao12;

            Ao5ValueTextBlock.Text = ao5.ToString();
            Ao12ValueTextBlock.Text = ao12.ToString();
        }

        private void RefreshStatusTextBlock() {
            StatusTextBlock.Text = timerStatus.ToString() == TimerStatus.Display.ToString() ? TimerStatus.Waiting.ToString() : timerStatus.ToString();
        }

        private void StartHoldingTick() {
            isHolding = true;
            TimerTextBlock.Foreground = YellowBrush;
            holdingCheckTimer.Start();
        }

        private void StartTimer() {
            startTime = DateTime.Now;
            refreshTimeTimer.Start();
        }

        private void RefreshTimeTimer_Tick(object sender, object e) {
            endTime = DateTime.Now;
            DisplayTime(endTime - startTime);
        }

        private void SwitchLeftSplitView() {
            PseudoHambergurMenu.IsPaneOpen = !PseudoHambergurMenu.IsPaneOpen;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            SwitchLeftSplitView();
        }

        private void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ResultListBoxItem.IsSelected) {
                InfoFrame.Navigate(typeof(ResultPage));
            }
            else if (SettingListBoxItem.IsSelected) {

            }
            else if (EmptyListBoxItem.IsSelected) {
                InfoFrame.Navigate(typeof(EmptyPage));
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e) {
        //    App.Results.Insert(0, new Result(new TimeSpan(12123415124), App.Results.Count + 1, 12.345, 67.890));
        //}
    }
}