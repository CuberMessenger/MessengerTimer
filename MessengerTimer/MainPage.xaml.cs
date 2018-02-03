using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UIHelper;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MessengerTimer {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    enum TimerStatus { Waiting, Display, Observing, Timing }

    public sealed partial class MainPage : Page {
        //Static Val
        private static Brush BlackBrush = new SolidColorBrush(Windows.UI.Colors.Black);
        private static Brush YellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private static Brush RedBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private static Brush GreenBrush = new SolidColorBrush(Windows.UI.Colors.Green);

        //Useful Var
        private TimerStatus TimerStatus { get; set; }
        private DispatcherTimer refreshTimeTimer { get; set; }

        //Display Var
        private DateTime startTime { get; set; }
        private DateTime endTime { get; set; }

        //Config Var
        private bool needObserving { get; set; }
        private long startDelay { get; set; }
        private string timerFormat { get; set; }

        public MainPage() {
            this.InitializeComponent();
            Init();
        }

        private void Init() {
            InitBingBackground();
            InitConfig();
            InitUI();

            TimerStatus = TimerStatus.Waiting;

            refreshTimeTimer = new DispatcherTimer();
            refreshTimeTimer.Interval = new TimeSpan(10000);
            refreshTimeTimer.Tick += RefreshTimeTimer_Tick;

            Window.Current.CoreWindow.KeyUp += Space_KeyUp;
            Window.Current.CoreWindow.KeyDown += Space_KeyDown;
        }

        private void InitBingBackground() {
            var image = new ImageBrush();
            image.ImageSource = new BitmapImage(new Uri(BingImage.FetchUrlAsync(), UriKind.Absolute));
            BackGroundGrid.Background = image;
        }

        private void InitConfig() {
            needObserving = true;
            startDelay = 3000000;
            timerFormat = "s.fff";
        }

        private void InitUI() {
            StatusTextBlock.Text = TimerStatus.ToString();
            DisplayTime(new TimeSpan(0));
        }

        private void DisplayTime(TimeSpan timeSpan) {
            TimerTextBlock.Text = new DateTime(timeSpan.Ticks).ToString(timerFormat);
        }

        private void StopTimer() {
            endTime = DateTime.Now;
            DisplayTime(endTime - startTime);
            refreshTimeTimer.Stop();

            //Todo
        }

        private void RefreshStatusTextBlock() {
            StatusTextBlock.Text = TimerStatus.ToString() == TimerStatus.Display.ToString() ? TimerStatus.Waiting.ToString() : TimerStatus.ToString();
        }

        private void Space_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            if (args.VirtualKey == Windows.System.VirtualKey.Space) {
                switch (TimerStatus) {
                    case TimerStatus.Timing:
                        TimerStatus = TimerStatus.Display;
                        StopTimer();
                        //Todo
                        break;
                    default:
                        break;
                }
                TimerTextBlock.Foreground = TimerStatus == TimerStatus.Display ? RedBrush : YellowBrush;
                RefreshStatusTextBlock();
            }
        }

        private void StartTimer() {
            startTime = DateTime.Now;
            refreshTimeTimer.Start();
            //Todo
        }

        private void RefreshTimeTimer_Tick(object sender, object e) {
            endTime = DateTime.Now;
            DisplayTime(endTime - startTime);
        }

        private void Space_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            if (args.VirtualKey == Windows.System.VirtualKey.Space) {
                switch (TimerStatus) {
                    case TimerStatus.Waiting:

                        if (needObserving) {
                            TimerStatus = TimerStatus.Observing;
                            //Todo
                        }
                        else {
                            TimerStatus = TimerStatus.Timing;
                            StartTimer();
                            //Todo
                        }
                        break;
                    case TimerStatus.Observing:
                        TimerStatus = TimerStatus.Timing;
                        StartTimer();
                        //Todo
                        break;
                    case TimerStatus.Display:
                        TimerStatus = TimerStatus.Waiting;
                        //Todo
                        break;
                    default:
                        break;
                }
                TimerTextBlock.Foreground = BlackBrush;
                RefreshStatusTextBlock();
            }
        }
    }
}