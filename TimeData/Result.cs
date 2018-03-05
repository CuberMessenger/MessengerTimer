using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeData {
    public class Result {
        public double resultValue { get; set; }
        public int Id { get; set; }
        public double ao5Value { get; set; }
        public double ao12Value { get; set; }

        public Result(TimeSpan timeSpan, int id) {
            Id = id;
            resultValue = double.Parse(new DateTime(timeSpan.Ticks).ToString("s.fff"));
        }

        public Result(int Id, double resultValue, double ao5Value, double ao12Value) {
            this.Id = Id;
            this.resultValue = resultValue;
            this.ao5Value = ao5Value;
            this.ao12Value = ao12Value;
        }
    }
}
