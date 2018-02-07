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

namespace MessengerTimer {
    public sealed partial class MainPage : Page {
        private void RealTimeSpaceKeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            if (args.VirtualKey == Windows.System.VirtualKey.Space) {
                switch (timerStatus) {
                    case TimerStatus.Timing:
                        timerStatus = TimerStatus.Display;
                        StopTimer();

                        TimerTextBlock.Foreground = RedBrush;
                        break;
                    case TimerStatus.Waiting:
                        if (!needObserving)
                            StartHoldingTick();
                        else
                            TimerTextBlock.Foreground = YellowBrush;
                        break;
                    case TimerStatus.Observing:
                        StartHoldingTick();
                        break;
                    case TimerStatus.Display:
                        TimerTextBlock.Foreground = RedBrush;
                        break;
                    default:
                        if (!(timerStatus == TimerStatus.Holding))
                            TimerTextBlock.Foreground = YellowBrush;
                        break;
                }
                RefreshStatusTextBlock();
            }
        }

        private void RealTimeSpaceKeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            if (args.VirtualKey == Windows.System.VirtualKey.Space) {
                switch (timerStatus) {
                    case TimerStatus.Waiting:
                        if (needObserving) {
                            timerStatus = TimerStatus.Observing;

                            ResetTimer();
                        }
                        else
                            isHolding = false;
                        break;
                    case TimerStatus.Holding:
                        timerStatus = TimerStatus.Timing;
                        StartTimer();
                        break;
                    case TimerStatus.Observing:
                        isHolding = false;
                        break;
                    case TimerStatus.Display:
                        timerStatus = TimerStatus.Waiting;
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
