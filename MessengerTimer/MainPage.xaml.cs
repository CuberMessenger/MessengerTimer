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
using min2phase;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    public enum DisplayModeEnum { RealTime, ToSecond, OnlyOberving, Hidden }

    public enum InfoFrameStatus { Null, Result, Empty, Setting }

    /*
     * Examples:
     * MMSSFF ->  12:34.56
     * MMSSFFF -> 12:34:567
     * SSFF -> 12.34
     * SSFFF -> 12.345
     */
    public enum TimerFormat { MMSSFF, MMSSFFF, SSFF, SSFFF }

    public sealed partial class MainPage : Page {
        //Static Val
        private static Brush BlackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        static public ObservableCollection<Result> Results = new ObservableCollection<Result>();
        static public AllResults allResult;

        //Useful Var
        private TimerStatus _timerStatus;
        private TimerStatus TimerStatus {
            get => _timerStatus;
            set {
                _timerStatus = value;
                if (value != TimerStatus.Display)
                    Bindings.Update();
            }
        }
        private DispatcherTimer RefreshTimeTimer { get; set; }
        private DispatcherTimer HoldingCheckTimer { get; set; }
        private bool IsHolding { get; set; }
        private InfoFrameStatus CurentInfoFrameStatus { get; set; }

        //Display Var
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        //Setting
        public static AppSettings appSettings = new AppSettings();

        public MainPage() {
            InitializeComponent();

            InitUI();

            InitResults();

            InitDisplay();

            InitHotKeys();
        }

        private async void InitBingBackgroundAsync() {
            var image = new ImageBrush {
                ImageSource = await BingImage.GetImageAsync(),
                Stretch = Stretch.UniformToFill
            };
            BackGroundGrid.Background = image;
        }

        private void InitUI() {
            InitBingBackgroundAsync();

            TimerStatus = TimerStatus.Waiting;
            StatusTextBlock.Text = TimerStatus.ToString();
            ResetTimer();

            CurentInfoFrameStatus = InfoFrameStatus.Null;

            ScrambleFrame.Navigate(typeof(ScramblePage));

            GenerateNewScramble();
        }

        private void InitHotKeys() {
            //Hot Key
            Window.Current.CoreWindow.KeyUp += EscapeKeyUp;
        }

        private async Task ReadSaveDataAsync() {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            StorageFile file = await storageFolder.CreateFileAsync("SaveData", CreationCollisionOption.OpenIfExists);

            string data = await FileIO.ReadTextAsync(file);

            if (String.IsNullOrWhiteSpace(data)) {
                allResult = new AllResults {
                    ResultGroups = new ObservableCollection<ResultGroup>() {
                        new ResultGroup() {
                            GroupName = "3x3",
                            Results = new ObservableCollection<Result>()
                        }
                    }
                };
            }
            else {
                allResult = AllResults.FromJson(data);
            }
        }

        private void FillResult(ResultGroup resultGroup) {
            foreach (var item in resultGroup.Results) {
                Results.Add(item);
            }

            RefreshAoNResults();
        }

        private async void InitResults() {
            await ReadSaveDataAsync();

            FillResult(allResult.ResultGroups[appSettings.CurrentDataGroupIndex]);
        }

        private void InitDisplay() {
            HoldingCheckTimer = new DispatcherTimer { Interval = new TimeSpan(appSettings.StartDelay) };
            HoldingCheckTimer.Tick += HoldingCheckTimer_Tick;

            switch (appSettings.DisplayMode) {
                case DisplayModeEnum.RealTime:
                    RefreshTimeTimer = new DispatcherTimer {
                        Interval = new TimeSpan(10000)
                    };
                    RefreshTimeTimer.Tick += RefreshTimeTimer_Tick;

                    Window.Current.CoreWindow.KeyUp += TimerControlSpaceKeyUp;
                    Window.Current.CoreWindow.KeyDown += TimerControlSpaceKeyDown;
                    break;
                case DisplayModeEnum.OnlyOberving:
                    //Todo
                    break;
                default:
                    break;
            }
        }

        private void ResetTimer() => DisplayTime(Result.GetFormattedString(0));

        public static async void SaveDataAsync(bool isDelete) {
            if (!isDelete) {
                allResult.ResultGroups[appSettings.CurrentDataGroupIndex].Results.Clear();
                foreach (var item in Results)
                    allResult.ResultGroups[appSettings.CurrentDataGroupIndex].Results.Add(item);
            }

            string json = allResult.ToJson();

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("SaveData", CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(file, json);
        }

        public void UpdateResult(Result result, int index = 0) {
            Results.Insert(index, result);

            RefreshListOfResult(index);
            SaveDataAsync(false);
        }

        public void RefreshAoNResults() {
            try {
                Ao5ValueTextBlock.Text = Results.First().Ao5String;
                Ao12ValueTextBlock.Text = Results.First().Ao12String;
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

        public void DeleteResult(int index) {
            Results.RemoveAt(index--);

            RefreshListOfResult(index);
            SaveDataAsync(false);
        }

        private void MainPageNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.IsSettingsInvoked) {
                //Todo
            }
            else {
                switch (args.InvokedItem) {
                    case "Results":
                        if (CurentInfoFrameStatus == InfoFrameStatus.Result) {
                            InfoFrame.Navigate(typeof(EmptyPage));
                            CurentInfoFrameStatus = InfoFrameStatus.Empty;
                        }
                        else {
                            InfoFrame.Navigate(typeof(ResultPage));
                            CurentInfoFrameStatus = InfoFrameStatus.Result;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void GenerateNewScramble() {
            string cube = Tools.randomCube();
            string scramble = new Search().solution(cube, 21, 1000000, 0, Search.INVERSE_SOLUTION);

            ScrambleTextBlock.Text = scramble;
            (ScrambleFrame.Content as ScramblePage).RefreshScramble(cube);
        }

        private async void ScrambleTestButton_Click(object sender, RoutedEventArgs e) {
            GenerateNewScramble();
        }

        private void ScrambleFrame_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e) => ScrambleFrame.Opacity = 0.4;

        private void ScrambleFrame_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e) =>
                ScrambleFrame.Margin = new Thickness(
                ScrambleFrame.Margin.Left, ScrambleFrame.Margin.Top,
                Math.Clamp(ScrambleFrame.Margin.Right - e.Delta.Translation.X, 0, MainGrid.ActualWidth - 360),
                Math.Clamp(ScrambleFrame.Margin.Bottom - e.Delta.Translation.Y, 0, MainGrid.ActualHeight - 280));

        private void ScrambleFrame_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e) => ScrambleFrame.Opacity = 0.8;
    }
}