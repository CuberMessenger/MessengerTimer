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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    enum TimerStatus { Waiting, Display, Observing, Holding, Timing }

    enum DisplayMode { RealTime, OnlyOberving }

    public sealed partial class MainPage : Page
    {
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
        private List<DataGroup> DataGroups { get; set; }

        //Display Var
        private DateTime StartTime { get; set; }
        private DateTime EndTime { get; set; }

        //Config Var
        private bool NeedObserving { get; set; }
        private long StartDelay { get; set; }
        private string TimerFormat { get; set; }
        private DisplayMode DisplayMode { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Init();
        }

        private void InitConfig()
        {
            NeedObserving = false;
            StartDelay = 3000000;
            TimerFormat = "s.fff";
            DisplayMode = DisplayMode.RealTime;
        }

        private async void InitBingBackgroundAsync()
        {
            var image = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(await BingImage.FetchUrlAsync(), UriKind.Absolute))
            };
            BackGroundGrid.Background = image;
        }

        private void InitUI()
        {
            InitBingBackgroundAsync();
            StatusTextBlock.Text = TimerStatus.ToString();
            ResetTimer();
        }

        private void ParseSaveData(string raw)
        {
            var rawArray = raw.Split(' ');

            int index = 0;
            for (int i = 0; i < int.Parse(rawArray.First()); i++)
            {
                DataGroup dataGroup = new DataGroup { Type = rawArray[++index], Count = int.Parse(rawArray[++index]), Results = new System.Collections.ObjectModel.ObservableCollection<Result>() };

                for (int j = 0; j < dataGroup.Count; j++)
                    dataGroup.Results.Add(new Result(j + 1, double.Parse(rawArray[++index]), double.Parse(rawArray[++index]), double.Parse(rawArray[++index])));

                DataGroups.Add(dataGroup);
            }
        }

        private async Task ReadSaveDataAsync()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file;

            try
            {
                file = await storageFolder.GetFileAsync("SaveData");
            }
            catch (FileNotFoundException)
            {
                await storageFolder.CreateFileAsync("SaveData");
                file = await storageFolder.GetFileAsync("SaveData");
            }

            await FileIO.WriteTextAsync(file, "1 3x3 3 1.23 2.34 3.45 4.567 5.678 6.789 7.89 8.90 9.00");

            string data = await FileIO.ReadTextAsync(file);
            ParseSaveData(data);
        }

        private void FillResult(DataGroup dataGroup)
        {
            App.Results = dataGroup.Results;
        }

        private async void InitResults()
        {
            DataGroups = new List<DataGroup>();

            await ReadSaveDataAsync();

            FillResult(DataGroups.First());
        }

        private void Init()
        {
            InitConfig();
            InitUI();
            InitResults();

            TimerStatus = TimerStatus.Waiting;

            HoldingCheckTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(StartDelay)
            };
            HoldingCheckTimer.Tick += HoldingCheckTimer_Tick;

            //toDelete
            App.Results = new System.Collections.ObjectModel.ObservableCollection<Result>();
            //

            switch (DisplayMode)
            {
                case DisplayMode.RealTime:
                    RefreshTimeTimer = new DispatcherTimer
                    {
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

        private void EscapeKeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.Escape && TimerStatus == TimerStatus.Waiting)
                ResetTimer();
        }

        private void HoldingCheckTimer_Tick(object sender, object e)
        {
            if (IsHolding)
            {
                TimerStatus = TimerStatus.Holding;
                TimerTextBlock.Foreground = GreenBrush;
            }
            IsHolding = false;
            HoldingCheckTimer.Stop();
        }

        private void ResetTimer()
        {
            DisplayTime(new TimeSpan(0));
        }

        private void DisplayTime(TimeSpan timeSpan)
        {
            TimerTextBlock.Text = new DateTime(timeSpan.Ticks).ToString(TimerFormat);
        }

        private void StopTimer()
        {
            EndTime = DateTime.Now;
            DisplayTime(EndTime - StartTime);
            RefreshTimeTimer.Stop();

            UpdateResult(EndTime - StartTime);
        }

        private void UpdateResult(TimeSpan result)
        {
            App.Results.Insert(0, new Result(result, App.Results.Count + 1));

            double ao5 = 0, ao12 = 0;

            if (App.Results.Count >= 5)
            {
                for (int i = 0; i < 5; i++)
                    ao5 += App.Results[i].ResultValue;
                ao5 = Math.Round(ao5 / 5, 3);
            }
            else
                ao5 = double.NaN;

            if (App.Results.Count >= 12)
            {
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

        private void RefreshStatusTextBlock()
        {
            StatusTextBlock.Text = TimerStatus.ToString() == TimerStatus.Display.ToString() ? TimerStatus.Waiting.ToString() : TimerStatus.ToString();
        }

        private void StartHoldingTick()
        {
            IsHolding = true;
            TimerTextBlock.Foreground = YellowBrush;
            HoldingCheckTimer.Start();
        }

        private void StartTimer()
        {
            StartTime = DateTime.Now;
            RefreshTimeTimer.Start();
        }

        private void RefreshTimeTimer_Tick(object sender, object e)
        {
            EndTime = DateTime.Now;
            DisplayTime(EndTime - StartTime);
        }

        private void SwitchLeftSplitView()
        {
            PseudoHambergurMenu.IsPaneOpen = !PseudoHambergurMenu.IsPaneOpen;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchLeftSplitView();
        }

        private void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultListBoxItem.IsSelected)
            {
                InfoFrame.Navigate(typeof(ResultPage));
            }
            else if (SettingListBoxItem.IsSelected)
            {

            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e) {
        //    App.Results.Insert(0, new Result(new TimeSpan(12123415124), App.Results.Count + 1, 12.345, 67.890));
        //}
    }
}