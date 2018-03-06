using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerTimer.Models
{
    public class Result
    {
        public double ResultValue { get; set; }
        public int Id { get; set; }
        public double Ao5Value { get; set; }
        public double Ao12Value { get; set; }

        public Result() { }

        public Result(TimeSpan timeSpan, int id)
        {
            Id = id;
            ResultValue = double.Parse(new DateTime(timeSpan.Ticks).ToString("s.fff"));
        }

        public Result(int Id, double resultValue, double ao5Value, double ao12Value)
        {
            this.Id = Id;
            ResultValue = resultValue;
            Ao5Value = ao5Value;
            Ao12Value = ao12Value;
        }
    }
}
