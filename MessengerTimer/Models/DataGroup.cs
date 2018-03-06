using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerTimer.Models
{
    public class DataGroup
    {
        public ObservableCollection<Result> Results { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
    }
}
