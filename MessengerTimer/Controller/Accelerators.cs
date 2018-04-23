using MessengerTimer.DataModels;
using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MessengerTimer {
    public sealed partial class MainPage : Page {
        private void EscShotcut_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args) {
            if (TimerStatus == TimerStatus.Holding) {
                TimerStatus = TimerStatus.Display;
            }
            else if (TimerStatus == TimerStatus.Observing) {
                TimerStatus = TimerStatus.Waiting;
            }
            ResetTimer();
        }
    }
}