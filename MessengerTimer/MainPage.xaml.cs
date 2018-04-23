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
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    public enum DisplayModeEnum { RealTime, ToSecond, OnlyOberving, Hidden }

    public enum InfoFrameStatus { Null, Result, Empty, Setting, Formula, TipsAndAbout }

    public enum AverageType { Average, Mean }

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

        private static TimeSpan MilliSecondTimeSpan = new TimeSpan(100000);

        public static ObservableCollection<Result> Results = new ObservableCollection<Result>();
        public static AllResults allResult;

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
        private bool IsHolding { get; set; }
        private InfoFrameStatus CurrentInfoFrameStatus { get; set; }
        private Punishment CurrentResultPunishment { get; set; }
        /*
            Open app -> Click NextScramble 3 Times -> Click PreviousScramble 1 Time -> Stacks looks like beneath
            |       |       |       |
            |  s3   |       |       |
            |  s2   |       |       |
            |  s1   |       |  s4   |
            |____|       |____|
            Before      After
        */
        private Stack<Tuple<string, string>> BeforeScramblesStack { get; set; }
        private Stack<Tuple<string, string>> AfterScramblesStack { get; set; }
        private DispatcherTimer TextBlockFadeOutTimer { get; set; }
        private DispatcherTimer TextBlockFadeInTimer { get; set; }
        public Grid MainGridPointer { get; private set; }

        //Display Var
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }
        private string ScrambleToBeDisplay { get; set; }

        //Setting
        public AppSettings appSettings = new AppSettings();

        private void InitVars() {
            App.MainPageInstance = this;

            TimerStatus = TimerStatus.Waiting;
            RefreshTimeTimer = new DispatcherTimer { Interval = MilliSecondTimeSpan };
            CurrentInfoFrameStatus = InfoFrameStatus.Null;

            BeforeScramblesStack = new Stack<Tuple<string, string>>();
            AfterScramblesStack = new Stack<Tuple<string, string>>();

            TextBlockFadeOutTimer = new DispatcherTimer { Interval = MilliSecondTimeSpan };
            TextBlockFadeInTimer = new DispatcherTimer { Interval = MilliSecondTimeSpan };

            MainGridPointer = MainGrid;
        }

        public MainPage() {
            InitializeComponent();

            InitVars();

            InitUI();

            InitResults();

            InitDisplay();

            InitAccelerators();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            throw new NotImplementedException();
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

            ResetTimer();

            ScrambleFrame.Navigate(typeof(ScramblePage));

            InfoFrame.Navigate(typeof(EmptyPage));
            CurrentInfoFrameStatus = InfoFrameStatus.Empty;

            NextScramble();

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e) => 滴汤Button.Focus(FocusState.Keyboard);

        private void InitAccelerators() {
            //Accelerators
            var EscAccelerator = new Windows.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.Escape };
            var CtrlDAccelerator = new Windows.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.D, Modifiers = Windows.System.VirtualKeyModifiers.Control };

            EscAccelerator.Invoked += EscShotcut_Invoked;
            CtrlDAccelerator.Invoked += CtrlDAccelerator_Invoked;

            MainGrid.KeyboardAccelerators.Add(EscAccelerator);
            MainGrid.KeyboardAccelerators.Add(CtrlDAccelerator);
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

            FillStart:
            try {
                if (appSettings.CurrentDataGroupIndex >= 0 && appSettings.CurrentDataGroupIndex < allResult.ResultGroups.Count)
                    FillResult(allResult.ResultGroups[appSettings.CurrentDataGroupIndex]);
                else {
                    appSettings.CurrentDataGroupIndex = 0;
                    goto FillStart;
                }
            }
            catch (Exception) {
                Debug.Assert(false, "SaveData seems broken");
            }
        }

        private void InitDisplay() {
            RefreshTimeTimer.Tick += RefreshTimeTimer_Tick;

            TextBlockFadeOutTimer.Tick += TextBlockFadeOutTimer_Tick;
            TextBlockFadeInTimer.Tick += TextBlockFadeInTimer_Tick;

            Window.Current.CoreWindow.KeyUp += TimerControlSpaceKeyUp;
            Window.Current.CoreWindow.KeyDown += TimerControlSpaceKeyDown;
        }

        public void ResetTimer() => DisplayTime(Result.GetFormattedString(0));

        public static async void SaveDataAsync(bool isDelete) {
            if (!isDelete) {
                allResult.ResultGroups[App.MainPageInstance.appSettings.CurrentDataGroupIndex].Results.Clear();
                foreach (var item in Results)
                    allResult.ResultGroups[App.MainPageInstance.appSettings.CurrentDataGroupIndex].Results.Add(item);
            }

            string json = allResult.ToJson();

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("SaveData", CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(file, json);
        }

        public void UpdateResult(Result result, int index = 0) {
            Results.Insert(index, result);

            RefreshListOfResult(index, Results);

            new Thread(() => { SaveDataAsync(false); }).Start();

            if (InfoFrame.Content is ResultPage)
                (InfoFrame.Content as ResultPage).UpdateUI();
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

        private double CalcAoNValue(int startIndex, int N, ObservableCollection<Result> source) {
            double aoN = 0;

            if (source.Count - startIndex >= N) {
                List<Result> validResults = null;
                var window = source.Skip(startIndex).Take(N);
                var NumOfDNF = window.Where(rst => rst.ResultPunishment == Punishment.DNF).Count();

                if (NumOfDNF >= 2)
                    return -1;//means DNF
                else if (NumOfDNF == 1) {
                    switch (appSettings.AverageType) {
                        case AverageType.Average:
                            validResults = window.ToList();
                            validResults.Remove(window.Where(rst => rst.ResultPunishment == Punishment.DNF).First());
                            break;
                        case AverageType.Mean:
                            return -1;//means DNF
                    }
                }
                else {
                    validResults = window.OrderByDescending(rst => rst.ResultValue).ToList();
                    if (appSettings.AverageType == AverageType.Average) {
                        validResults.RemoveAt(0);
                    }
                }

                for (int i = 0; i < validResults.Count; i++)
                    aoN += validResults[i].ResultValue + (validResults[i].ResultPunishment == Punishment.PlusTwo ? 2 : 0);
                aoN /= validResults.Count;
            }
            else
                aoN = double.NaN;

            return aoN;
        }

        public void RefreshListOfResult(int index, ObservableCollection<Result> source) {
            for (int i = index; i >= 0; i--) {
                source[i].Id = source.Count - i;
                source[i].Ao5Value = CalcAoNValue(i, 5, source);
                source[i].Ao12Value = CalcAoNValue(i, 12, source);
            }

            RefreshAoNResults();
        }

        public void ReCalcAllAoN() {
            foreach (var resultGroup in allResult.ResultGroups) {
                RefreshListOfResult(resultGroup.Results.Count - 1, resultGroup.Results);
            }
            SaveDataAsync(isDelete: false);
        }

        public void DeleteResult(int index) {
            if (Results.Count <= 0) {
                return;
            }

            Results.RemoveAt(index--);

            RefreshListOfResult(index, Results);
            SaveDataAsync(false);
        }

        private void WithdrawInfoFrame() {
            InfoFrame.Navigate(typeof(EmptyPage), null, new EntranceNavigationTransitionInfo());
            CurrentInfoFrameStatus = InfoFrameStatus.Empty;
        }

        private static Dictionary<string, InfoFrameStatus> StringToInfoFrameStatus = new Dictionary<string, InfoFrameStatus> {
            { "Result", InfoFrameStatus.Result },
            { "Setting", InfoFrameStatus.Setting },
            { "Formula", InfoFrameStatus.Formula},
            { "Tips and About", InfoFrameStatus.TipsAndAbout}
        };

        private static Dictionary<string, Type> StringToPageType = new Dictionary<string, Type> {
            { "Result", typeof(ResultPage) },
            { "Setting", typeof(SettingPage) },
            { "Formula", typeof(FormulaPage) },
            { "Tips and About",typeof(TipsAndAboutPage)}
        };

        private void NavigateOrWithdraw(InfoFrameStatus pageStatusEnum, Type page) {
            if (CurrentInfoFrameStatus == pageStatusEnum)
                WithdrawInfoFrame();
            else {
                InfoFrame.Navigate(page, null, new EntranceNavigationTransitionInfo());
                CurrentInfoFrameStatus = pageStatusEnum;
            }
        }

        private void MainPageNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
            if (args.InvokedItem != null) {
                NavigateOrWithdraw(StringToInfoFrameStatus[args.InvokedItem as string], StringToPageType[args.InvokedItem as string]);
            }
        }

        private void SetScrambleTextBlockByAnimation(string scramble) {
            ScrambleToBeDisplay = scramble;
            TextBlockFadeOutTimer.Start();
        }

        private void TextBlockFadeInTimer_Tick(object sender, object e) {
            if (ScrambleTextBlock.Opacity < 1)
                ScrambleTextBlock.Opacity += 0.34;
            else
                TextBlockFadeInTimer.Stop();
        }

        private void TextBlockFadeOutTimer_Tick(object sender, object e) {
            if (ScrambleTextBlock.Opacity > 0)
                ScrambleTextBlock.Opacity -= 0.34;
            else {
                TextBlockFadeOutTimer.Stop();
                ScrambleTextBlock.Text = ScrambleToBeDisplay;
                TextBlockFadeInTimer.Start();
            }
        }

        private void NavigateToCurrentScramble() {
            var current = BeforeScramblesStack.Peek();
            (ScrambleFrame.Content as ScramblePage).RefreshScramble(current.Item1);
            //ScrambleTextBlock.Text = current.Item2;
            SetScrambleTextBlockByAnimation(current.Item2);
        }

        private void NextScramble(bool needNew = false) {
            if (ScrambleTextBlock.Opacity != 1)
                return;

            if (needNew && AfterScramblesStack.Count > 0) {
                var current = BeforeScramblesStack.Pop();
                while (AfterScramblesStack.Count > 0)
                    BeforeScramblesStack.Push(AfterScramblesStack.Pop());
                BeforeScramblesStack.Push(current);
            }

            BeforeScramblesStack.Push(AfterScramblesStack.Count == 0 ? GenerateNewScramble() : AfterScramblesStack.Pop());
            NavigateToCurrentScramble();
        }

        private void PreviousScramble() {
            if (ScrambleTextBlock.Opacity != 1)
                return;

            if (BeforeScramblesStack.Count == 1) {
                AfterScramblesStack.Push(BeforeScramblesStack.Pop());
                BeforeScramblesStack.Push(GenerateNewScramble());
            }
            else
                AfterScramblesStack.Push(BeforeScramblesStack.Pop());
            NavigateToCurrentScramble();
        }

        private Tuple<string, string> GenerateNewScramble() {
            string cube = Tools.randomCube();
            string scramble = new Search().solution(cube, 21, 1000000, 0, Search.INVERSE_SOLUTION);
            return new Tuple<string, string>(cube, scramble);
        }

        private void ScrambleFrame_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e) => ScrambleFrame.Opacity = 0.4;

        private void ScrambleFrame_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e) =>
                ScrambleFrame.Margin = new Thickness(
                ScrambleFrame.Margin.Left, ScrambleFrame.Margin.Top,
                Math.Clamp(ScrambleFrame.Margin.Right - e.Delta.Translation.X, 0, MainGrid.ActualWidth - 360),
                Math.Clamp(ScrambleFrame.Margin.Bottom - e.Delta.Translation.Y, 0, MainGrid.ActualHeight - 280));

        private void ScrambleFrame_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e) => ScrambleFrame.Opacity = 0.8;

        private void RelativePanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            PreviousScrambleButton.Opacity = 1;
            NextScrambleButton.Opacity = 1;
        }

        private void RelativePanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            PreviousScrambleButton.Opacity = 0;
            NextScrambleButton.Opacity = 0;
        }

        private void ScrambleTextBlock_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            if (e.GetCurrentPoint(ScrambleRelativePanel).Properties.MouseWheelDelta < 0)
                NextScramble();
            else
                PreviousScramble();
        }

        private void PreviousScrambleButton_Click(object sender, RoutedEventArgs e) => PreviousScramble();

        private void NextScrambleButton_Click(object sender, RoutedEventArgs e) => NextScramble();

        public void ReloadInfoFramePage(Type page) => InfoFrame.Navigate(page, null, new EntranceNavigationTransitionInfo());

        public void BindingsUpdate() => Bindings.Update();

    }
}