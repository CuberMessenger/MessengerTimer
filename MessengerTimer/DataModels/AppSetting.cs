using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MessengerTimer.DataModels {
    public class AppSettings : INotifyPropertyChanged {
        //Config Var
        public bool NeedObserving {
            get {
                return ReadSettings(nameof(NeedObserving), true);
            }
            set {
                SaveSettings(nameof(NeedObserving), value);
                NotifyPropertyChanged();
            }
        }

        public long StartDelay {
            get {
                return ReadSettings(nameof(StartDelay), (long)3000000);
            }
            set {
                SaveSettings(nameof(StartDelay), value);
                NotifyPropertyChanged();
            }
        }

        public TimerFormat TimerFormat {
            get {
                return ReadSettings(nameof(TimerFormat), TimerFormat.SSFFF);
            }
            set {
                SaveSettings(nameof(TimerFormat), (int)value);
                NotifyPropertyChanged();
            }
        }

        public DisplayModeEnum DisplayMode {
            get {
                return ReadSettings(nameof(DisplayMode), DisplayModeEnum.RealTime);
            }
            set {
                SaveSettings(nameof(DisplayMode), (int)value);
                NotifyPropertyChanged();
            }
        }

        public int CurrentDataGroupIndex {
            get {
                return ReadSettings(nameof(CurrentDataGroupIndex), 0);
            }
            set {
                SaveSettings(nameof(CurrentDataGroupIndex), value);
                NotifyPropertyChanged();
            }
        }

        public DateTime LastUpdateImageTime {
            get {
                return ReadSettings(nameof(LastUpdateImageTime), DateTimeOffset.MinValue).DateTime;
            }
            set {
                SaveSettings(nameof(LastUpdateImageTime), (DateTimeOffset)value);
            }
        }

        public Visibility ScrambleFrameVisibility => ShowScrambleState ? Visibility.Visible : Visibility.Collapsed;

        public bool ShowScrambleState {
            get {
                return ReadSettings(nameof(ShowScrambleState), true);
            }
            set {
                SaveSettings(nameof(ShowScrambleState), value);
                NotifyPropertyChanged("ScrambleFrameVisibility");
            }
        }

        public Visibility ScrambleTextVisibility => ShowScrambleText ? Visibility.Visible : Visibility.Collapsed;

        public bool ShowScrambleText {
            get {
                return ReadSettings(nameof(ShowScrambleText), true);
            }
            set {
                SaveSettings(nameof(ShowScrambleText), value);
                NotifyPropertyChanged("ScrambleFrameVisibility");
            }
        }

        public Visibility AverageTextVisibility => ShowAverageText ? Visibility.Visible : Visibility.Collapsed;

        public bool ShowAverageText {
            get {
                return ReadSettings(nameof(ShowAverageText), true);
            }
            set {
                SaveSettings(nameof(ShowAverageText), value);
                NotifyPropertyChanged("AverageTextVisibility");
            }
        }

        public AverageType AverageType {
            get {
                return ReadSettings(nameof(AverageType), AverageType.Average);
            }
            set {
                SaveSettings(nameof(AverageType), (int)value);
                NotifyPropertyChanged();
            }
        }

        public int ScrambleFontSize {
            get {
                return ReadSettings(nameof(ScrambleFontSize), 24);
            }
            set {
                SaveSettings(nameof(ScrambleFontSize), value);
                NotifyPropertyChanged();
            }
        }

        public bool SettingPageDefaultZoomOut {
            get {
                return ReadSettings(nameof(SettingPageDefaultZoomOut), false);
            }
            set {
                SaveSettings(nameof(SettingPageDefaultZoomOut), value);
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush MainGridBackgroundBrush => new SolidColorBrush(BackgroundTint);

        public Windows.UI.Color BackgroundTint {
            get => Windows.UI.Color.FromArgb(
                            (byte)((BackgroundTintARGB & 0xFF000000) >> 24),
                            (byte)((BackgroundTintARGB & 0x00FF0000) >> 16),
                            (byte)((BackgroundTintARGB & 0x0000FF00) >> 8),
                            (byte)(BackgroundTintARGB & 0x000000FF));
            set {
                uint color = value.A;
                color <<= 0b0000_1000; color |= value.R;
                color <<= 0b0000_1000; color |= value.G;
                color <<= 0b0000_1000; color |= value.B;
                BackgroundTintARGB = color;
            }
        }

        public uint BackgroundTintARGB {
            get {
                return ReadSettings(nameof(BackgroundTintARGB), 0xFFFFFFFF);
            }
            set {
                SaveSettings(nameof(BackgroundTintARGB), value);
                NotifyPropertyChanged("BackgroundTint");
                NotifyPropertyChanged("MainGridBackgroundBrush");
            }
        }

        public ApplicationDataContainer LocalSettings { get; set; }

        public AppSettings() => LocalSettings = ApplicationData.Current.LocalSettings;

        private void SaveSettings(string key, object value) => LocalSettings.Values[key] = value;

        private T ReadSettings<T>(string key, T defaultValue) {
            if (LocalSettings.Values.ContainsKey(key)) {
                return (T)LocalSettings.Values[key];
            }
            if (null != defaultValue) {
                return defaultValue;
            }
            return default(T);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

}
