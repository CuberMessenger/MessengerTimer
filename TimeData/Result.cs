using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeData {
    public class Result {
        public TimeSpan result { get; set; }
        public int Id { get; set; }
        public double resultValue { get; set; }
        public double ao5Value { get; set; }
        public double ao12Value { get; set; }
    }
}
