using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Puzzles;
using static TNoodle.Puzzles.CubePuzzle;

namespace MessengerTimer {
    public enum ScrambleType { x3, x2, x4, x5, x6, x7 }
    internal class ScrambleGenerator {
        internal static int ScrambleOrder => int.Parse(ScrambleType.ToString().Substring(1));
        internal static ScrambleType ScrambleType { get; set; }
        private static Random Rand { get; set; }

        static ScrambleGenerator() {
            Rand = new Random();
        }

        internal static (string, string) Generate() {
            if ((int)ScrambleType <= 5) {
                return GenerateCubePuzzle();
            }

            return ("", "");
        }

        private static (string, string) GenerateCubePuzzle() {
            var puzle = new CubePuzzle(ScrambleOrder);
            string scramble = puzle.GenerateWcaScramble(Rand);
            string cube = ((new CubeState(puzle)).ApplyAlgorithm(scramble) as CubeState).ToFaceCube();
            return (cube, scramble);
        }
    }
}
