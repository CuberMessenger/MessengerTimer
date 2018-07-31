using System;

namespace CSMinTwoPhase {
    public class Tools {
        public static bool Inited { get; private set; } = false;
        private static int[] initState = new int[2];
        private static int[] require = new int[] { 0x0, 0x1, 0x2, 0x2, 0x2, 0x7, 0xa, 0x3, 0x13, 0x13, 0x3, 0x6e, 0xca, 0xa6, 0x612, 0x512 };
        internal const bool USE_TWIST_FLIP_PRUN = true;

        private static void initIdx(int idx) {
            switch (idx) {
                case 0:
                    CubieCube.initMove();
                    goto case 1;
                case 1:
                    CubieCube.initSym();
                    goto case 2;
                case 2:
                    CubieCube.initFlipSym2Raw();
                    goto case 3;
                case 3:
                    CubieCube.initTwistSym2Raw();
                    goto case 4;
                case 4:
                    CubieCube.initPermSym2Raw();
                    goto case 5;
                case 5:
                    CoordCube.initFlipMove();
                    goto case 6;
                case 6:
                    CoordCube.initTwistMove();
                    goto case 7;
                case 7:
                    CoordCube.initUDSliceMoveConj();
                    goto case 8;
                case 8:
                    CoordCube.initCPermMove();
                    goto case 9;
                case 9:
                    CoordCube.initEPermMove();
                    goto case 10;
                case 10:
                    CoordCube.initMPermMoveConj();
                    goto case 11;
                case 11:
                    if (USE_TWIST_FLIP_PRUN) {
                        CoordCube.initTwistFlipPrun();
                    }
                    goto case 12;
                case 12:
                    CoordCube.initSliceTwistPrun();
                    goto case 13;
                case 13:
                    CoordCube.initSliceFlipPrun();
                    goto case 14;
                case 14:
                    CoordCube.initMEPermPrun();
                    goto case 15;
                case 15:
                    CoordCube.initMCPermPrun();
                    break;
            }
        }

        protected internal Tools() {
        }

        public static void init() {
            lock (typeof(Tools)) {
                if (Inited) {
                    return;
                }
                for (int i = 0; i <= 15; i++) {
                    initIdx(i);
                }

                Inited = true;
            }
        }


        private static readonly Random r = new Random();
        public static string randomCube() {
            return randomCube(r);
        }

        public static string randomCube(Random gen) {
            return randomState(STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, STATE_RANDOM, gen);
        }

        private static int resolveOri(sbyte[] arr, int @base, Random gen) {
            int sum = 0, idx = 0, lastUnknown = -1;
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] == -1) {
                    arr[i] = (sbyte)gen.Next(@base);
                    lastUnknown = i;
                }
                sum += arr[i];
            }
            if (sum % @base != 0 && lastUnknown != -1) {
                arr[lastUnknown] = (sbyte)((30 + arr[lastUnknown] - sum) % @base);
            }
            for (int i = 0; i < arr.Length - 1; i++) {
                idx *= @base;
                idx += arr[i];
            }
            return idx;
        }

        private static int countUnknown(sbyte[] arr) {
            if (arr == STATE_SOLVED) {
                return 0;
            }
            int cnt = 0;
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] == -1) {
                    cnt++;
                }
            }
            return cnt;
        }

        private static int resolvePerm(sbyte[] arr, int cntU, int parity, Random gen) {
            if (arr == STATE_SOLVED) {
                return 0;
            }
            else if (arr == STATE_RANDOM) {
                return parity == -1 ? gen.Next(2) : parity;
            }
            sbyte[] val = new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] != -1) {
                    val[arr[i]] = -1;
                }
            }
            int idx = 0;
            for (int i = 0; i < arr.Length; i++) {
                if (val[i] != -1) {
                    int j = gen.Next(idx + 1);
                    sbyte temp = val[i];
                    val[idx++] = val[j];
                    val[j] = temp;
                }
            }
            int last = -1;
            for (idx = 0; idx < arr.Length && cntU > 0; idx++) {
                if (arr[idx] == -1) {
                    if (cntU == 2) {
                        last = idx;
                    }
                    arr[idx] = val[--cntU];
                }
            }
            int p = Util.getNParity(Util.getNPerm(arr, arr.Length), arr.Length);
            if (p == 1 - parity && last != -1) {
                sbyte temp = arr[idx - 1];
                arr[idx - 1] = arr[last];
                arr[last] = temp;
            }
            return p;
        }

        public static readonly sbyte[] STATE_RANDOM = null;
        public static readonly sbyte[] STATE_SOLVED = new sbyte[0];

        protected internal static string randomState(sbyte[] cp, sbyte[] co, sbyte[] ep, sbyte[] eo, Random gen) {
            int parity;
            int cntUE = ep == STATE_RANDOM ? 12 : countUnknown(ep);
            int cntUC = cp == STATE_RANDOM ? 8 : countUnknown(cp);
            int cpVal, epVal;
            if (cntUE < 2) { //ep != STATE_RANDOM
                if (ep == STATE_SOLVED) {
                    epVal = parity = 0;
                }
                else {
                    parity = resolvePerm(ep, cntUE, -1, gen);
                    epVal = Util.getNPerm(ep, 12);
                }
                if (cp == STATE_SOLVED) {
                    cpVal = 0;
                }
                else if (cp == STATE_RANDOM) {
                    do {
                        cpVal = gen.Next(40320);
                    } while (Util.getNParity(cpVal, 8) != parity);
                }
                else {
                    resolvePerm(cp, cntUC, parity, gen);
                    cpVal = Util.getNPerm(cp, 8);
                }
            }
            else { //ep != STATE_SOLVED
                if (cp == STATE_SOLVED) {
                    cpVal = parity = 0;
                }
                else if (cp == STATE_RANDOM) {
                    cpVal = gen.Next(40320);
                    parity = Util.getNParity(cpVal, 8);
                }
                else {
                    parity = resolvePerm(cp, cntUC, -1, gen);
                    cpVal = Util.getNPerm(cp, 8);
                }
                if (ep == STATE_RANDOM) {
                    do {
                        epVal = gen.Next(479001600);
                    } while (Util.getNParity(epVal, 12) != parity);
                }
                else {
                    resolvePerm(ep, cntUE, parity, gen);
                    epVal = Util.getNPerm(ep, 12);
                }
            }
            return Util.toFaceCube(new CubieCube(cpVal, co == STATE_RANDOM ? gen.Next(2187) : (co == STATE_SOLVED ? 0 : resolveOri(co, 3, gen)), epVal, eo == STATE_RANDOM ? gen.Next(2048) : (eo == STATE_SOLVED ? 0 : resolveOri(eo, 2, gen))));
        }

        public static string randomLastLayer() => randomLastLayer(r);
        public static string randomLastLayer(Random gen) => randomState(new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 }, new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);

        public static string randomLastSlot() => randomLastSlot(r);

        public static string randomLastSlot(Random gen) => randomState(new sbyte[] { -1, -1, -1, -1, -1, 5, 6, 7 }, new sbyte[] { -1, -1, -1, -1, -1, 0, 0, 0 }, new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, 9, 10, 11 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, 0, 0, 0 }, gen);

        public static string randomZBLastLayer() => randomZBLastLayer(r);

        public static string randomZBLastLayer(Random gen) => randomState(new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 }, new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 }, STATE_SOLVED, gen);

        public static string randomCornerOfLastLayer() => randomCornerOfLastLayer(r);

        public static string randomCornerOfLastLayer(Random gen) => randomState(new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0 }, STATE_SOLVED, STATE_SOLVED, gen);

        public static string randomEdgeOfLastLayer() => randomEdgeOfLastLayer(r);
        public static string randomEdgeOfLastLayer(Random gen) => randomState(STATE_SOLVED, STATE_SOLVED, new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, 8, 9, 10, 11 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0 }, gen);

        public static string randomCrossSolved() => randomCrossSolved(r);

        public static string randomCrossSolved(Random gen) => randomState(STATE_RANDOM, STATE_RANDOM, new sbyte[] { -1, -1, -1, -1, 4, 5, 6, 7, -1, -1, -1, -1 }, new sbyte[] { -1, -1, -1, -1, 0, 0, 0, 0, -1, -1, -1, -1 }, gen);

        public static string randomEdgeSolved() => randomEdgeSolved(r);

        public static string randomEdgeSolved(Random gen) => randomState(STATE_RANDOM, STATE_RANDOM, STATE_SOLVED, STATE_SOLVED, gen);

        public static string randomCornerSolved() => randomCornerSolved(r);

        public static string randomCornerSolved(Random gen) => randomState(STATE_SOLVED, STATE_SOLVED, STATE_RANDOM, STATE_RANDOM, gen);

        public static string superFlip() => Util.toFaceCube(new CubieCube(0, 0, 0, 2047));

        //         -1: There is not exactly one facelet of each colour<br>
        //         -2: Not all 12 edges exist exactly once<br>
        //         -3: Flip error: One edge has to be flipped<br>
        //         -4: Not all 8 corners exist exactly once<br>
        //         -5: Twist error: One corner has to be twisted<br>
        //         -6: Parity error: Two corners or two edges have to be exchanged </returns>
        public static int verify(string facelets) => (new Search()).verify(facelets);
    }
}