using System;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Threading.Tasks;
using MessengerTimer.DataModels;
using System.Text;
using System.Collections.ObjectModel;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    public enum DisplayModeEnum { RealTime, OnlyOberving }

    enum InfoFrameStatus { Null, Result, Empty, Setting }

    public sealed partial class MainPage : Page {
        //Static Val
        private static Brush BlackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        static public ObservableCollection<Result> Results = new ObservableCollection<Result>();
        static public ObservableCollection<DataGroup> DataGroups;

        //Useful Var
        private TimerStatus TimerStatus { get; set; }
        private DispatcherTimer RefreshTimeTimer { get; set; }
        private DispatcherTimer HoldingCheckTimer { get; set; }
        private bool IsHolding { get; set; }
        private InfoFrameStatus infoFrameStatus { get; set; }

        //Display Var
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        //Setting
        public static AppSettings appSettings = new AppSettings();

        public MainPage() {
            InitializeComponent();
            Init();
        }

        private async void InitBingBackgroundAsync() {
            var image = new ImageBrush {
                ImageSource = new BitmapImage(new Uri(await BingImage.FetchUrlAsync(), UriKind.Absolute)),
                Stretch = Stretch.UniformToFill
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
                DataGroup dataGroup = new DataGroup { Type = rawArray[++index], Results = new ObservableCollection<Result>() };

                var nor = int.Parse(rawArray[++index]);
                for (int j = 0; j < nor; j++)
                    dataGroup.Results.Add(new Result(nor - j, double.Parse(rawArray[++index]), double.Parse(rawArray[++index]), double.Parse(rawArray[++index])));

                DataGroups.Add(dataGroup);
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
                await FileIO.WriteTextAsync(file, "1 3x3 0");
            }

            string data = await FileIO.ReadTextAsync(file);
            ParseSaveData(data);
            DataGroup.CurrentDataGroup = DataGroups[appSettings.CurrentDataGroupIndex];
        }

        private void FillResult(DataGroup dataGroup) {
            foreach (var item in dataGroup.Results) {
                Results.Add(item);
            }
            //Results = dataGroup.Results;

            try {
                Ao5ValueTextBlock.Text = Results.First().Ao5Value.ToString();
                Ao12ValueTextBlock.Text = Results.First().Ao12Value.ToString();
            }
            catch (Exception) {
                Ao5ValueTextBlock.Text = double.NaN.ToString();
                Ao12ValueTextBlock.Text = double.NaN.ToString();
            }
        }

        private async void InitResults() {
            DataGroups = new ObservableCollection<DataGroup>();

            //appSettings.CurrentDataGroupIndex = 0;
            await ReadSaveDataAsync();

            FillResult(DataGroups.First());
        }

        private void Init() {
            InitUI();
            InitResults();

            TimerStatus = TimerStatus.Waiting;

            HoldingCheckTimer = new DispatcherTimer {
                Interval = new TimeSpan(appSettings.StartDelay)
            };
            HoldingCheckTimer.Tick += HoldingCheckTimer_Tick;

            switch (appSettings.DisplayMode) {
                case DisplayModeEnum.RealTime:
                    RefreshTimeTimer = new DispatcherTimer {
                        Interval = new TimeSpan(10000)
                    };
                    RefreshTimeTimer.Tick += RefreshTimeTimer_Tick;

                    Window.Current.CoreWindow.KeyUp += RealTimeSpaceKeyUp;
                    Window.Current.CoreWindow.KeyDown += RealTimeSpaceKeyDown;
                    break;
                case DisplayModeEnum.OnlyOberving:
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
            TimerTextBlock.Text = new DateTime(timeSpan.Ticks).ToString(appSettings.TimerFormat);
        }

        public static async void SaveDataAsync(bool isDelete) {
            if (!isDelete) {
                DataGroups[appSettings.CurrentDataGroupIndex].Results.Clear();
                foreach (var item in Results)
                    DataGroups[appSettings.CurrentDataGroupIndex].Results.Add(item);
            }

            StringBuilder buffer = new StringBuilder();
            buffer.Append(DataGroups.Count);

            for (int i = 0; i < DataGroups.Count; i++) {
                buffer.Append(" " + DataGroups[i].Type);
                buffer.Append(" " + DataGroups[i].Results.Count);

                for (int j = 0; j < DataGroups[i].Results.Count; j++)
                    buffer.Append(" " + DataGroups[i].Results[j].ResultValue + " " + DataGroups[i].Results[j].Ao5Value + " " + DataGroups[i].Results[j].Ao12Value);
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
        }

        public void RefreshAoNResults() {
            try {
                Ao5ValueTextBlock.Text = Results.First().Ao5Value.ToString();
                Ao12ValueTextBlock.Text = Results.First().Ao12Value.ToString();
            }
            catch (Exception) {
                Ao5ValueTextBlock.Text = double.NaN.ToString();
                Ao12ValueTextBlock.Text = double.NaN.ToString();
            }
        }

        private double CalcAoNValue(int startIndex, int N) {
            double aoN = 0;

            if (Results.Count - startIndex >= N) {
                for (int i = 0; i < N; i++)
                    aoN += Results[i + startIndex].ResultValue;
                aoN = Math.Round(aoN / N, 3);
            }
            else
                aoN = double.NaN;

            return aoN;
        }

        public void RefreshListOfResult(int index) {
            for (int i = index; i >= 0; i--) {
                Results[i].Id = Results.Count - i;
                Results[i].Ao5Value = CalcAoNValue(i, 5);
                Results[i].Ao12Value = CalcAoNValue(i, 12);
            }

            RefreshAoNResults();
        }

        private void UpdateResult(TimeSpan result, int index = 0) {
            Results.Insert(index, new Result(result, Results.Count + 1));

            RefreshListOfResult(index);
            SaveDataAsync(false);
        }

        public void DeleteResult(int index) {
            Results.RemoveAt(index--);

            RefreshListOfResult(index);
            SaveDataAsync(false);
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