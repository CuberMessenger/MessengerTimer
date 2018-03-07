using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MessengerTimer.Models
{
    public class AppSettings : INotifyPropertyChanged
    {
        //Config Var
        public bool NeedObserving
        {
            get
            {
                return ReadSettings(nameof(NeedObserving), false);
            }
            set
            {
                SaveSettings(nameof(NeedObserving), value);
                NotifyPropertyChanged();
            }
        }

        public long StartDelay
        {
            get
            {
                return ReadSettings(nameof(StartDelay), 3000000);
            }
            set
            {
                SaveSettings(nameof(StartDelay), value);
                NotifyPropertyChanged();
            }
        }

        public string TimerFormat
        {
            get
            {
                return ReadSettings(nameof(TimerFormat), "s.fff");
            }
            set
            {
                SaveSettings(nameof(TimerFormat), value);
                NotifyPropertyChanged();
            }
        }

        public DisplayModeEnum DisplayMode
        {
            get
            {
                return (DisplayModeEnum)ReadSettings(nameof(DisplayMode), 0);
            }
            set
            {
                SaveSettings(nameof(DisplayModeEnum), value);
                NotifyPropertyChanged();
            }
        }

        public int CurrentDataGroupIndex
        {
            get
            {
                return ReadSettings(nameof(CurrentDataGroupIndex), 0);
            }
            set
            {
                SaveSettings(nameof(CurrentDataGroupIndex), value);
                NotifyPropertyChanged();
            }
        }


        public ApplicationDataContainer LocalSettings { get; set; }

        public AppSettings()
        {
            LocalSettings = ApplicationData.Current.LocalSettings;
        }

        private void SaveSettings(string key, object value)
        {
            LocalSettings.Values[key] = value;
        }

        private T ReadSettings<T>(string key, T defaultValue)
        {
            if (LocalSettings.Values.ContainsKey(key))
            {
                return (T)LocalSettings.Values[key];
            }
            if (null != defaultValue)
            {
                return defaultValue;
            }
            return default(T);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

}
