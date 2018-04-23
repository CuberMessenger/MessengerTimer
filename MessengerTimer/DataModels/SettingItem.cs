using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MessengerTimer.DataModels {
    public enum InputControlTypes { ToggleSwitch, Slider, ComboBox, ColorPicker }
    public class SettingItem {
        private static AppSettings appSettings = App.MainPageInstance.appSettings;

        private static List<string> displayModeEnums = new List<string>
        { DisplayModeEnum.RealTime.ToString(), DisplayModeEnum.ToSecond.ToString(), DisplayModeEnum.OnlyOberving.ToString(), DisplayModeEnum.Hidden.ToString() };

        private static List<string> timerFormats = new List<string>
        { TimerFormat.MMSSFF.ToString(), TimerFormat.MMSSFFF.ToString(), TimerFormat.SSFF.ToString(), TimerFormat.SSFFF.ToString() };

        private static List<string> averageTypes = new List<string>
        {AverageType.Average.ToString(), AverageType.Mean.ToString() };

        private static InputControlTypes[] ICTs = new InputControlTypes[] { InputControlTypes.ToggleSwitch, InputControlTypes.Slider, InputControlTypes.ComboBox, InputControlTypes.ColorPicker };
        public string Title { get; set; }
        public Dictionary<InputControlTypes, Visibility> InputControlVisibility { get; set; }

        public bool IsToggleSwitchOn {
            get {
                switch (Title) {
                    case "NeedObserving: ":
                        return appSettings.NeedObserving;
                    case "ShowScambleState: ":
                        return appSettings.ShowScrambleState;
                    case "ShowScrambleText: ":
                        return appSettings.ShowScrambleText;
                    case "ShowAverageText: ":
                        return appSettings.ShowAverageText;
                    case "SettingPageDefaultZoomOut: ":
                        return appSettings.SettingPageDefaultZoomOut;
                    default:
                        return false;
                }
            }
            set {
                switch (Title) {
                    case "NeedObserving: ":
                        appSettings.NeedObserving = value;
                        break;
                    case "ShowScambleState: ":
                        appSettings.ShowScrambleState = value;
                        break;
                    case "ShowScrambleText: ":
                        appSettings.ShowScrambleText = value;
                        break;
                    case "ShowAverageText: ":
                        appSettings.ShowAverageText = value;
                        break;
                    case "SettingPageDefaultZoomOut: ":
                        appSettings.SettingPageDefaultZoomOut = value;
                        break;
                    default:
                        break;
                }
                App.MainPageInstance.BindingsUpdate();
            }
        }

        public double SliderMinimum {
            get {
                switch (Title) {
                    case "StartDelay: ":
                        return 0.0;
                    case "ScrambleFontSize: ":
                        return 0;
                    case "ScrambleFrameSize :":
                        return 0;
                    default:
                        return 0.0;
                }
            }
        }

        public double SliderMaximum {
            get {
                switch (Title) {
                    case "StartDelay: ":
                        return 3.0;
                    case "ScrambleFontSize: ":
                        return 60;
                    case "ScrambleFrameSize :":
                        return 1;
                    default:
                        return 0.0;
                }
            }
        }

        public double SliderStepFrequency {
            get {
                switch (Title) {
                    case "StartDelay: ":
                        return 0.05;
                    case "ScrambleFontSize: ":
                        return 1;
                    case "ScrambleFrameSize :":
                        return 0.01;
                    default:
                        return 0.1;
                }
            }
        }

        public double SliderValue {
            get {
                switch (Title) {
                    case "StartDelay: ":
                        return appSettings.StartDelay / 10000000.0;
                    case "ScrambleFontSize: ":
                        return appSettings.ScrambleFontSize - 1;
                    case "ScrambleFrameSize :":
                        return appSettings.ScrambleFrameSizeCoefficient - 0.5;
                    default:
                        return 0;
                }
            }
            set {
                switch (Title) {
                    case "StartDelay: ":
                        appSettings.StartDelay = (long)(value * 10000000);
                        break;
                    case "ScrambleFontSize: ":
                        appSettings.ScrambleFontSize = (int)(value + 1);
                        break;
                    case "ScrambleFrameSize :":
                        appSettings.ScrambleFrameSizeCoefficient = value + 0.5;
                        break;
                    default:
                        break;
                }
            }
        }

        public object ComboBoxItemSource {
            get {
                switch (Title) {
                    case "DisplayMode: ":
                        return displayModeEnums;
                    case "TimerFormat: ":
                        return timerFormats;
                    case "AverageType: ":
                        return averageTypes;
                    default:
                        return null;
                }
            }
        }

        public int ComboBoxSelectedIndex {
            get {
                switch (Title) {
                    case "DisplayMode: ":
                        return displayModeEnums.IndexOf(appSettings.DisplayMode.ToString());
                    case "TimerFormat: ":
                        return timerFormats.IndexOf(appSettings.TimerFormat.ToString());
                    case "AverageType: ":
                        return averageTypes.IndexOf(appSettings.AverageType.ToString());
                    default:
                        return 0;
                }
            }
            set {
                switch (Title) {
                    case "DisplayMode: ":
                        appSettings.DisplayMode = (DisplayModeEnum)value;
                        break;
                    case "TimerFormat: ":
                        appSettings.TimerFormat = (TimerFormat)value;
                        break;
                    case "AverageType: ":
                        appSettings.AverageType = (AverageType)value;
                        App.MainPageInstance.ReCalcAllAoN();
                        break;
                    default:
                        break;
                }
            }
        }

        public Windows.UI.Color BackgroundTint {
            get {
                switch (Title) {
                    case "BackgroundTint: ":
                        return appSettings.BackgroundTint;
                    default:
                        return Windows.UI.Colors.White;
                }
            }
            set {
                switch (Title) {
                    case "BackgroundTint: ":
                        appSettings.BackgroundTint = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public SettingItem(string title, InputControlTypes visibleControl) {
            Title = title;
            InputControlVisibility = new Dictionary<InputControlTypes, Visibility>();
            foreach (InputControlTypes t in ICTs)
                InputControlVisibility.Add(t, Visibility.Collapsed);
            InputControlVisibility[visibleControl] = Visibility.Visible;
        }

        public Visibility GetControlVisibility(InputControlTypes visibleControl) => InputControlVisibility[visibleControl];
    }

    public class SettingItemGroup {
        public string Class { get; set; }

        public ObservableCollection<SettingItem> Items { get; set; }

        public static List<SettingItemGroup> Instance { get; private set; }

        static SettingItemGroup() {
            Instance = new List<SettingItemGroup>();

            var timingGroup = new SettingItemGroup { Class = "Timing", Items = new ObservableCollection<SettingItem>() };
            timingGroup.Items.Add(new SettingItem(title: "NeedObserving: ", visibleControl: InputControlTypes.ToggleSwitch));
            timingGroup.Items.Add(new SettingItem(title: "StartDelay: ", visibleControl: InputControlTypes.Slider));
            timingGroup.Items.Add(new SettingItem(title: "DisplayMode: ", visibleControl: InputControlTypes.ComboBox));

            var scrambleGroup = new SettingItemGroup { Class = "Scramble", Items = new ObservableCollection<SettingItem>() };
            scrambleGroup.Items.Add(new SettingItem(title: "ShowScrambleText: ", visibleControl: InputControlTypes.ToggleSwitch));
            scrambleGroup.Items.Add(new SettingItem(title: "ShowScambleState: ", visibleControl: InputControlTypes.ToggleSwitch));
            scrambleGroup.Items.Add(new SettingItem(title: "ScrambleFontSize: ", visibleControl: InputControlTypes.Slider));


            var statisticsGroup = new SettingItemGroup { Class = "Statistics", Items = new ObservableCollection<SettingItem>() };
            statisticsGroup.Items.Add(new SettingItem(title: "ShowAverageText: ", visibleControl: InputControlTypes.ToggleSwitch));
            statisticsGroup.Items.Add(new SettingItem(title: "AverageType: ", visibleControl: InputControlTypes.ComboBox));


            var userInterfaceGroup = new SettingItemGroup { Class = "UserInterface", Items = new ObservableCollection<SettingItem>() };
            userInterfaceGroup.Items.Add(new SettingItem(title: "SettingPageDefaultZoomOut: ", visibleControl: InputControlTypes.ToggleSwitch));
            userInterfaceGroup.Items.Add(new SettingItem(title: "ScrambleFrameSize :", visibleControl: InputControlTypes.Slider));
            userInterfaceGroup.Items.Add(new SettingItem(title: "TimerFormat: ", visibleControl: InputControlTypes.ComboBox));
            userInterfaceGroup.Items.Add(new SettingItem(title: "BackgroundTint: ", visibleControl: InputControlTypes.ColorPicker));

            Instance.Add(timingGroup);
            Instance.Add(scrambleGroup);
            Instance.Add(statisticsGroup);
            Instance.Add(userInterfaceGroup);
        }
    }
}
