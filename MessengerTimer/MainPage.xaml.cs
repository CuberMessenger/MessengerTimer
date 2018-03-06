using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using MessengerTimer.Models;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    enum DisplayMode { RealTime, OnlyOberving }

    enum InfoFrameStatus { Null, Result, Empty, Setting }

    public sealed partial class MainPage : Page {
        //Static Val
        private static Brush BlackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        //Useful Var
        private TimerStatus TimerStatus { get; set; }
        private DispatcherTimer RefreshTimeTimer { get; set; }
        private DispatcherTimer HoldingCheckTimer { get; set; }
        private bool IsHolding { get; set; }
        private InfoFrameStatus infoFrameStatus { get; set; }

        //Display Var
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        //Config Var
        private bool NeedObserving { get; set; }
        private long StartDelay { get; set; }
        private string TimerFormat { get; set; }
        private DisplayMode DisplayMode { get; set; }

        public MainPage() {
            InitializeComponent();
            Init();
        }

        private void InitConfig() {
            NeedObserving = false;
            StartDelay = 3000000;
            TimerFormat = "s.fff";
            DisplayMode = DisplayMode.RealTime;
        }

        private async void InitBingBackgroundAsync() {
            var image = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(await BingImage.FetchUrlAsync(), UriKind.Absolute))
            };
            BackGroundGrid.Background = image;
        }

        private void InitUI() {
            InitBingBackgroundAsync();
            StatusTextBlock.Text = TimerStatus.ToString();
            ResetTimer();

            infoFrameStatus = InfoFrameStatus.Null;
        }

        private void ParseSaveData(string raw) {
            var rawArray = raw.Split(' ');

            int index = 0;
            for (int i = 0; i < int.Parse(rawArray.First()); i++) {
                DataGroup dataGroup = new DataGroup { Type = rawArray[++index], Count = int.Parse(rawArray[++index]), Results = new System.Collections.ObjectModel.ObservableCollection<Result>() };

                for (int j = 0; j < dataGroup.Count; j++)
                    dataGroup.Results.Add(new Result(dataGroup.Count - j, double.Parse(rawArray[++index]), double.Parse(rawArray[++index]), double.Parse(rawArray[++index])));

                App.DataGroups.Add(dataGroup);
            }
        }

        private async Task ReadSaveDataAsync() {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try {
                file = await storageFolder.GetFileAsync("SaveData");
            }
            catch (FileNotFoundException) {
                await storageFolder.CreateFileAsync("SaveData");
                file = await storageFolder.GetFileAsync("SaveData");
            }

            string data = await FileIO.ReadTextAsync(file);
            ParseSaveData(data);
        }

        private void FillResult(DataGroup dataGroup) {
            App.Results = dataGroup.Results;

            try {
                Ao5ValueTextBlock.Text = App.Results.First().Ao5Value.ToString();
                Ao12ValueTextBlock.Text = App.Results.First().Ao12Value.ToString();
            }
            catch (Exception) {
                Ao5ValueTextBlock.Text = double.NaN.ToString();
                Ao12ValueTextBlock.Text = double.NaN.ToString();
            }
        }

        private async void InitResults() {
            App.DataGroups = new System.Collections.ObjectModel.ObservableCollection<DataGroup>();

            await ReadSaveDataAsync();

            FillResult(App.DataGroups.First());
        }

        private void Init() {
            InitConfig();
            InitUI();
            InitResults();

            TimerStatus = TimerStatus.Waiting;

            HoldingCheckTimer = new DispatcherTimer {
                Interval = new TimeSpan(StartDelay)
            };
            HoldingCheckTimer.Tick += HoldingCheckTimer_Tick;

            //toDelete
            App.Results = new System.Collections.ObjectModel.ObservableCollection<Result>();
            //

            switch (DisplayMode) {
                case DisplayMode.RealTime:
                    RefreshTimeTimer = new DispatcherTimer {
                        Interval = new TimeSpan(10000)
                    };
                    RefreshTimeTimer.Tick += RefreshTimeTimer_Tick;

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
            if (args.VirtualKey == Windows.System.VirtualKey.Escape && TimerStatus == TimerStatus.Waiting)
                ResetTimer();
        }

        private void HoldingCheckTimer_Tick(object sender, object e) {
            if (IsHolding) {
                TimerStatus = TimerStatus.Holding;
                TimerTextBlock.Foreground = GreenBrush;
            }
            IsHolding = false;
            HoldingCheckTimer.Stop();
        }

        private void ResetTimer() {
            DisplayTime(new TimeSpan(0));
        }

        private void DisplayTime(TimeSpan timeSpan) {
            TimerTextBlock.Text = new DateTime(timeSpan.Ticks).ToString(TimerFormat);
        }

        private async void SaveData() {
            App.DataGroups.First().Results = App.Results;
            App.DataGroups.First().Count = App.DataGroups.First().Results.Count;

            StringBuilder buffer = new StringBuilder();
            buffer.Append(App.DataGroups.Count);

            for (int i = 0; i < App.DataGroups.Count; i++) {
                buffer.Append(" " + App.DataGroups[i].Type);
                buffer.Append(" " + App.DataGroups[i].Count);

                for (int j = 0; j < App.DataGroups[i].Count; j++)
                    buffer.Append(" " + App.DataGroups[i].Results[j].ResultValue + " " + App.DataGroups[i].Results[j].Ao5Value + " " + App.DataGroups[i].Results[j].Ao12Value);
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
            EndTime = DateTime.Now;
            DisplayTime(EndTime - StartTime);
            RefreshTimeTimer.Stop();

            UpdateResult(EndTime - StartTime);
            SaveData();
        }

        private void UpdateResult(TimeSpan result) {
            App.Results.Insert(0, new Result(result, App.Results.Count + 1));

            double ao5 = 0, ao12 = 0;

            if (App.Results.Count >= 5) {
                for (int i = 0; i < 5; i++)
                    ao5 += App.Results[i].ResultValue;
                ao5 = Math.Round(ao5 / 5, 3);
            }
            else
                ao5 = double.NaN;

            if (App.Results.Count >= 12) {
                for (int i = 0; i < 12; i++)
                    ao12 += App.Results[i].ResultValue;
                ao12 = Math.Round(ao12 / 12, 3);
            }
            else
                ao12 = double.NaN;

            App.Results.First().Ao5Value = ao5;
            App.Results.First().Ao12Value = ao12;

            Ao5ValueTextBlock.Text = ao5.ToString();
            Ao12ValueTextBlock.Text = ao12.ToString();
        }

        private void RefreshStatusTextBlock() {
            StatusTextBlock.Text = TimerStatus.ToString() == TimerStatus.Display.ToString() ? TimerStatus.Waiting.ToString() : TimerStatus.ToString();
        }

        private void StartHoldingTick() {
            IsHolding = true;
            TimerTextBlock.Foreground = YellowBrush;
            HoldingCheckTimer.Start();
        }

        private void StartTimer() {
            StartTime = DateTime.Now;
            RefreshTimeTimer.Start();
        }

        private void RefreshTimeTimer_Tick(object sender, object e) {
            EndTime = DateTime.Now;
            DisplayTime(EndTime - StartTime);
        }

        private void MainPageNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                //Todo
            }
            else {
                switch (args.InvokedItem) {
                    case "Results":
                        if (infoFrameStatus == InfoFrameStatus.Result) {
                            InfoFrame.Navigate(typeof(EmptyPage));
                            infoFrameStatus = InfoFrameStatus.Empty;
                        }
                        else {
                            InfoFrame.Navigate(typeof(ResultPage));
                            infoFrameStatus = InfoFrameStatus.Result;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}