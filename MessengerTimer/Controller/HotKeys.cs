using MessengerTimer.DataModels;
using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MessengerTimer {
    public sealed partial class MainPage : Page {
        private void EscapeKeyUp(CoreWindow sender, KeyEventArgs args) {
            if (args.VirtualKey == VirtualKey.Escape) {
                if (TimerStatus == TimerStatus.Holding) {
                    TimerStatus = TimerStatus.Display;
                    ResetTimer();
                }
                else if (TimerStatus == TimerStatus.Observing) {
                    TimerStatus = TimerStatus.Waiting;
                    ResetTimer();
                }
            }
        }
    }
}