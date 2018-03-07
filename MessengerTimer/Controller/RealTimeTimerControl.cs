using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MessengerTimer
{
    public sealed partial class MainPage : Page
    {
        private void RealTimeSpaceKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Space)
            {
                switch (TimerStatus)
                {
                    case TimerStatus.Timing:
                        TimerStatus = TimerStatus.Display;
                        StopTimer();

                        TimerTextBlock.Foreground = RedBrush;
                        break;
                    case TimerStatus.Waiting:
                        if (!appSettings.NeedObserving)
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
                        if (!(TimerStatus == TimerStatus.Holding))
                            TimerTextBlock.Foreground = YellowBrush;
                        break;
                }
                RefreshStatusTextBlock();
                滴汤Button.Focus(FocusState.Keyboard);
            }
        }

        private void RealTimeSpaceKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Space)
            {
                switch (TimerStatus)
                {
                    case TimerStatus.Waiting:
                        if (appSettings.NeedObserving)
                        {
                            TimerStatus = TimerStatus.Observing;

                            ResetTimer();
                        }
                        else
                            IsHolding = false;
                        break;
                    case TimerStatus.Holding:
                        TimerStatus = TimerStatus.Timing;
                        StartTimer();
                        break;
                    case TimerStatus.Observing:
                        IsHolding = false;
                        break;
                    case TimerStatus.Display:
                        TimerStatus = TimerStatus.Waiting;
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
