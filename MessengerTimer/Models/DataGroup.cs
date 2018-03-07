using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MessengerTimer.Models {
    public class DataGroup : INotifyPropertyChanged {
        public ObservableCollection<Result> Results { get; set; }

        private string _type;
        public string Type {
            get => _type;
            set {
                _type = value;
                NotifyPropertyChanged();
            }
        }

        //private int _count;
        //public int Count {
        //    get => _count;
        //    set {
        //        _count = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        static public DataGroup CurrentDataGroup;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string propName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
