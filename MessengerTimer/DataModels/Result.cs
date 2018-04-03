using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        private Punishment _resultPunishment;

        public static string GetFormattedString(double value, Punishment punishment = Punishment.None) {
            if (value < 0)
                return "DNF";//when aoNValue need to be DNF, it will be assigned by -1
            switch (App.MainPageInstance.appSettings.TimerFormat) {
                case TimerFormat.MMSSFF:
                    //Todo
                    return string.Empty;
                case TimerFormat.MMSSFFF:
                    //Todo
                    return string.Empty;
                case TimerFormat.SSFF:
                    switch (punishment) {
                        case Punishment.None:
                            return value.ToString("F2");
                        case Punishment.PlusTwo:
                            return (value + 2).ToString("F2") + "+";
                        case Punishment.DNF:
                            return "DNF";
                        default:
                            return value.ToString("F2");
                    }
                case TimerFormat.SSFFF:
                    switch (punishment) {
                        case Punishment.None:
                            return value.ToString("F3");
                        case Punishment.PlusTwo:
                            return (value + 2).ToString("F3") + "+";
                        case Punishment.DNF:
                            return "DNF";
                        default:
                            return value.ToString("F3");
                    }
                default:
                    return string.Empty;
            }
        }

        public string ResultString => GetFormattedString(ResultValue, ResultPunishment);

        public string Ao5String => GetFormattedString(Ao5Value);

        public string Ao12String => GetFormattedString(Ao12Value);

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
            get => _resultPunishment;
            set {
                _resultPunishment = value;
                NotifyPropertyChanged("ResultString");
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

        public Result(double resultValue, int id, Punishment punishment) {
            Id = id;
            ResultPunishment = punishment;
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
