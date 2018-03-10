using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MessengerTimer.DataModels {
    public class Result : INotifyPropertyChanged {
        private int _id;
        private double _ao5Value;
        private double _ao12Value;
        private double _resultValue;

        public double ResultValue {
            get => _resultValue; set {
                _resultValue = value;
                NotifyPropertyChanged();
            }
        }
        public int Id {
            get => _id; set {
                _id = value;
                NotifyPropertyChanged();
            }
        }
        public double Ao5Value {
            get => _ao5Value; set {
                _ao5Value = value;
                NotifyPropertyChanged();
            }
        }
        public double Ao12Value {
            get => _ao12Value; set {
                _ao12Value = value;
                NotifyPropertyChanged();
            }
        }

        public Result() { }

        public Result(object resultValue, int id) {
            Id = id;
            if (resultValue is double)
                ResultValue = (double)resultValue;
            else if (resultValue is TimeSpan)
                ResultValue = double.Parse(new DateTime(((TimeSpan)resultValue).Ticks).ToString("s.fff"));
        }

        public Result(int id, double resultValue, double ao5Value, double ao12Value) {
            Id = id;
            ResultValue = resultValue;
            Ao5Value = ao5Value;
            Ao12Value = ao12Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
