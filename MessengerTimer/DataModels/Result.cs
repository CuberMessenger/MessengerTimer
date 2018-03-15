using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MessengerTimer.DataModels {
    public class AllResults {
        [JsonProperty(nameof(ResultGroups))]
        public ObservableCollection<ResultGroup> ResultGroups { get; set; }

        public static AllResults FromJson(string json) => JsonConvert.DeserializeObject<AllResults>(json, Converter.Settings);
    }

    public class ResultGroup : INotifyPropertyChanged {
        private string _groupName;

        [JsonProperty(nameof(GroupName))]
        public string GroupName {
            get => _groupName;
            set {
                _groupName = value;
                NotifyPropertyChanged();
            }
        }

        [JsonProperty(nameof(Results))]
        public ObservableCollection<Result> Results { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ResultGroup() { }

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public enum Punishment { None, PlusTwo, DNF }

    public class Result : INotifyPropertyChanged {
        private int _id;
        private double _ao5Value;
        private double _ao12Value;
        private double _resultValue;
        private Punishment _resltPunishment;

        public static string GetFormattedString(double value) {
            switch (MainPage.appSettings.TimerFormat) {
                case TimerFormat.MMSSFF:
                    //Todo
                    return string.Empty;
                case TimerFormat.MMSSFFF:
                    //Todo
                    return string.Empty;
                case TimerFormat.SSFF:
                    return value.ToString("F2");
                case TimerFormat.SSFFF:
                    return value.ToString("F3");
                default:
                    return string.Empty;
            }
        }

        [JsonProperty(nameof(ResultString))]
        public string ResultString {
            get => GetFormattedString(ResultValue);
        }

        [JsonProperty(nameof(Ao5String))]
        public string Ao5String {
            get => GetFormattedString(Ao5Value);
        }

        [JsonProperty(nameof(Ao12String))]
        public string Ao12String {
            get => GetFormattedString(Ao12Value);
        }

        [JsonProperty(nameof(ResultValue))]
        public double ResultValue {
            get => _resultValue;
            set {
                _resultValue = value;
                NotifyPropertyChanged("ResultString");
            }
        }

        [JsonProperty(nameof(ResultPunishment))]
        public Punishment ResultPunishment {
            get => _resltPunishment;
            set {
                _resltPunishment = value;
                NotifyPropertyChanged();
            }
        }

        [JsonProperty(nameof(Id))]
        public int Id {
            get => _id; set {
                _id = value;
                NotifyPropertyChanged();
            }
        }

        [JsonProperty(nameof(Ao5Value))]
        public double Ao5Value {
            get => _ao5Value; set {
                _ao5Value = value;
                NotifyPropertyChanged("Ao5String");
            }
        }

        [JsonProperty(nameof(Ao12Value))]
        public double Ao12Value {
            get => _ao12Value; set {
                _ao12Value = value;
                NotifyPropertyChanged("Ao12String");
            }
        }

        public Result() { }

        public Result(double resultValue, int id) {
            Id = id;
            ResultPunishment = Punishment.None;
            ResultValue = Math.Round(resultValue, 3);
        }


        public Result(int Id, double resultValue, double ao5Value, double ao12Value) {
            this.Id = Id;
            ResultValue = resultValue;
            Ao5Value = ao5Value;
            Ao12Value = ao12Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public static class Serialize {
        public static string ToJson(this AllResults self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal class Converter {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
