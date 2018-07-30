using min2phase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerTimer {
    public enum ScrambleType { x3, x2, x4, x5, x6, x7 }
    internal class ScrambleGenerator {
        internal static ScrambleType ScrambleType { get; set; }
        internal static (string, string) Generate() {
            switch (ScrambleType) {
                case ScrambleType.x3:
                    return Generate3x3();
                case ScrambleType.x2:
                    return ("RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR", "asd");
                    break;
                case ScrambleType.x4:
                    break;
                case ScrambleType.x5:
                    break;
                case ScrambleType.x6:
                    break;
                case ScrambleType.x7:
                    break;
                default:
                    break;
            }
            return ("", "");
        }

        private static (string, string) Generate3x3() {
            string cube = Tools.randomCube();
            string scramble = new Search().solution(cube, 21, 1000000, 0, Search.INVERSE_SOLUTION);
            return (cube, scramble);
        }
    }
}
