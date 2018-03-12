using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMin2Phase {
    class CoordCube {
        public static int N_MOVES = 18;
        public static int N_MOVES2 = 10;

        public static int N_SLICE = 495;
        public static int N_TWIST = 2187;
        public static int N_TWIST_SYM = 324;
        public static int N_FLIP = 2048;
        public static int N_FLIP_SYM = 336;
        public static int N_PERM = 40320;
        public static int N_PERM_SYM = 2768;
        public static int N_MPERM = 24;
        public static int N_COMB = 70;

        //XMove = Move Table
        //XPrun = Pruning Table
        //XConj = Conjugate Table

        //phase1
        public static char[,] UDSliceMove = new char[N_SLICE, N_MOVES];
        public static char[,] TwistMove = new char[N_TWIST_SYM, N_MOVES];
        public static char[,] FlipMove = new char[N_FLIP_SYM, N_MOVES];
        public static char[,] UDSliceConj = new char[N_SLICE, 8];
        public static int[] UDSliceTwistPrun = new int[N_SLICE * N_TWIST_SYM / 8 + 1];
        public static int[] UDSliceFlipPrun = new int[N_SLICE * N_FLIP_SYM / 8 + 1];
        public static int[] TwistFlipPrun = Search.USE_TWIST_FLIP_PRUN ? new int[N_FLIP * N_TWIST_SYM / 8 + 1] : null;

        //phase2
        public static char[,] CPermMove = new char[N_PERM_SYM, N_MOVES];
        public static char[,] EPermMove = new char[N_PERM_SYM, N_MOVES2];
        public static char[,] MPermMove = new char[N_MPERM, N_MOVES2];
        public static char[,] MPermConj = new char[N_MPERM, 16];
        public static char[,] CCombMove = new char[N_COMB, N_MOVES2];
        public static char[,] CCombConj = new char[N_COMB, 16];
        public static int[] MCPermPrun = new int[N_MPERM * N_PERM_SYM / 8 + 1];
        public static int[] MEPermPrun = new int[N_MPERM * N_PERM_SYM / 8 + 1];
        public static int[] EPermCCombPrun = new int[N_COMB * N_PERM_SYM / 8 + 1];

        /*
        *  0: not initialized, 1: partially initialized, 2: finished
        */
        static int initLevel = 0;

        public static void init() {
            if (initLevel == 2)
                return;

            if (initLevel == 0) {
                CubieCube.initPermSym2Raw();

                initCPermMove();
                initEPermMove();
                initMPermMoveConj();
                initCombMoveConj();

                CubieCube.initFlipSym2Raw();
                CubieCube.initTwistSym2Raw();
                initFlipMove();
                initTwistMove();
                initUDSliceMoveConj();
            }

            if (initPruning(initLevel == 0))
                initLevel = 2;
            else
                initLevel = 1;
        }

        static bool initPruning(bool isFirst) {
            bool initedPrun = true;

            initedPrun = (initedPrun || isFirst) && initMEPermPrun();
            initedPrun = (initedPrun || isFirst) && initMCPermPrun();
            initedPrun = (initedPrun || isFirst) && initPermCombPrun();
            initedPrun = (initedPrun || isFirst) && initSliceTwistPrun();
            initedPrun = (initedPrun || isFirst) && initSliceFlipPrun();

            if (Search.USE_TWIST_FLIP_PRUN)
                initedPrun = (initedPrun || isFirst) && initTwistFlipPrun();
            return initedPrun;
        }

        static void setPruning(int[] table, int index, int value) => table[index >> 3] ^= value << ((index & 7) << 2);

        public static int getPruning(int[] table, int index) => table[index >> 3] >> ((index & 7) << 2) & 0xf;

        static void initUDSliceMoveConj() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_SLICE; i++) {
                c.setUDSlice(i);

                for (int j = 0; j < N_MOVES; j += 3) {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[j], d);
                    UDSliceMove[i, j] = (char)d.getUDSlice();
                }

                for (int j = 0; j < 16; j += 2) {
                    CubieCube.EdgeConjugate(c, CubieCube.SymMultInv[0, j], d);
                    UDSliceConj[i, j >> 1] = (char)(d.getUDSlice() & 0x1ff);
                }
            }

            for (int i = 0; i < N_SLICE; i++) {
                for (int j = 0; j < N_MOVES; j += 3) {
                    int udslice = UDSliceMove[i, j];

                    for (int k = 1; k < 3; k++) {
                        int cx = UDSliceMove[udslice & 0x1ff, j];
                        udslice = Util.permMult[udslice >> 9, cx >> 9] << 9 | cx & 0x1ff;
                        UDSliceMove[i, j + k] = (char)udslice;
                    }
                }
            }
        }

        static void initFlipMove() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_FLIP_SYM; i++) {
                c.setFlip(CubieCube.FlipS2R[i]);

                for (int j = 0; j < N_MOVES; j++) {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[j], d);
                    FlipMove[i, j] = (char)d.getFlipSym();
                }
            }
        }

        static void initTwistMove() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_TWIST_SYM; i++) {
                c.setTwist(CubieCube.TwistS2R[i]);

                for (int j = 0; j < N_MOVES; j++) {
                    CubieCube.CornMult(c, CubieCube.moveCube[j], d);
                    TwistMove[i, j] = (char)d.getTwistSym();
                }
            }
        }

        static void initCPermMove() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_PERM_SYM; i++) {
                c.setCPerm(CubieCube.EPermS2R[i]);

                for (int j = 0; j < N_MOVES; j++) {
                    CubieCube.CornMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    CPermMove[i, j] = (char)d.getCPermSym();
                }
            }
        }

        static void initEPermMove() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_PERM_SYM; i++) {
                c.setEPerm(CubieCube.EPermS2R[i]);

                for (int j = 0; j < N_MOVES2; j++) {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    EPermMove[i, j] = (char)d.getEPermSym();
                }
            }
        }

        static void initMPermMoveConj() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_MPERM; i++) {
                c.setMPerm(i);

                for (int j = 0; j < N_MOVES2; j++) {
                    CubieCube.EdgeMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    MPermMove[i, j] = (char)d.getMPerm();
                }

                for (int j = 0; j < 16; j++) {
                    CubieCube.EdgeConjugate(c, CubieCube.SymMultInv[0, j], d);
                    MPermConj[i, j] = (char)d.getMPerm();
                }
            }
        }

        static void initCombMoveConj() {
            CubieCube c = new CubieCube();
            CubieCube d = new CubieCube();

            for (int i = 0; i < N_COMB; i++) {
                c.setCComb(i);

                for (int j = 0; j < N_MOVES2; j++) {
                    CubieCube.CornMult(c, CubieCube.moveCube[Util.ud2std[j]], d);
                    CCombMove[i, j] = (char)d.getCComb();
                }

                for (int j = 0; j < 16; j++) {
                    CubieCube.CornConjugate(c, CubieCube.SymMultInv[0, j], d);
                    CCombConj[i, j] = (char)d.getCComb();
                }
            }
        }

        static bool hasZero(int val) => ((val - 0x11111111) & ~val & 0x88888888) != 0;

        static bool initRawSymPrun(int[] PrunTable,
                                       char[,] RawMove, char[,] RawConj,
                                       char[,] SymMove, char[] SymState,
                                       int PrunFlag) {
            int SYM_SHIFT = PrunFlag & 0xf;
            int SYM_E2C_MAGIC = ((PrunFlag >> 4) & 1) == 1 ? CubieCube.SYM_E2C_MAGIC : 0x00000000;
            bool IS_PHASE2 = ((PrunFlag >> 5) & 1) == 1;
            int INV_DEPTH = PrunFlag >> 8 & 0xf;
            int MAX_DEPTH = PrunFlag >> 12 & 0xf;
            int MIN_DEPTH = PrunFlag >> 16 & 0xf;

            int SYM_MASK = (1 << SYM_SHIFT) - 1;
            bool ISTFP = RawMove == null;
            int N_RAW = ISTFP ? N_FLIP : RawMove.Length;
            int N_SIZE = N_RAW * SymMove.Length;
            int N_MOVES = IS_PHASE2 ? 10 : 18;
            int NEXT_AXIS_MAGIC = N_MOVES == 10 ? 0x42 : 0x92492;

            int depth = getPruning(PrunTable, N_SIZE) - 1;
            int done = 0;

            if (depth == -1) {
                for (int i = 0; i < N_SIZE / 8 + 1; i++)
                    PrunTable[i] = 0x11111111;

                setPruning(PrunTable, 0, 0 ^ 1);
                depth = 0;
                done = 1;
            }

            int SEARCH_DEPTH = Search.PARTIAL_INIT_LEVEL > 0 ?
                                    Math.Min(Math.Max(depth + 1, MIN_DEPTH), MAX_DEPTH) : MAX_DEPTH;

            while (depth < SEARCH_DEPTH) {
                int mask = (int)((depth + 1) * 0x11111111 ^ 0xffffffff);

                for (int i = 0; i < PrunTable.Length; i++) {
                    int _val = PrunTable[i] ^ mask;
                    _val &= _val >> 1;
                    PrunTable[i] += _val & (_val >> 2) & 0x11111111;
                }

                bool inv = depth > INV_DEPTH;
                int select = inv ? (depth + 2) : depth;
                int selArrMask = select * 0x11111111;
                int check = inv ? depth : (depth + 2); depth++;
                int xorVal = depth ^ (depth + 1);
                int val = 0;

                for (int i = 0; i < N_SIZE; i++, val >>= 4) {
                    if ((i & 7) == 0) {
                        val = PrunTable[i >> 3];

                        if (!hasZero(val ^ selArrMask)) {
                            i += 7;
                            continue;
                        }
                    }

                    if ((val & 0xf) != select)
                        continue;

                    int raw = i % N_RAW;
                    int sym = i / N_RAW;
                    int flip = 0, fsym = 0;

                    if (ISTFP) {
                        flip = CubieCube.FlipR2S[raw];
                        fsym = flip & 7;
                        flip >>= 3;
                    }

                    for (int m = 0; m < N_MOVES; m++) {
                        int symx = SymMove[sym, m];
                        int rawx;

                        if (ISTFP)
                            rawx = CubieCube.FlipS2RF[
                                       FlipMove[flip, CubieCube.Sym8Move[m << 3 | fsym]] ^
                                       fsym ^ (symx & SYM_MASK)];
                        else
                            rawx = RawConj[RawMove[raw, m] & 0x1ff, symx & SYM_MASK];

                        symx >>= SYM_SHIFT;
                        int idx = symx * N_RAW + rawx;
                        int prun = getPruning(PrunTable, idx);

                        if (prun != check) {
                            if (prun < depth - 1)
                                m += NEXT_AXIS_MAGIC >> m & 3;
                            continue;
                        }

                        done++;
                        if (inv) {
                            setPruning(PrunTable, i, xorVal);
                            break;
                        }

                        setPruning(PrunTable, idx, xorVal);
                        for (int j = 1, symState = SymState[symx]; (symState >>= 1) != 0; j++) {
                            if ((symState & 1) != 1)
                                continue;

                            int idxx = symx * N_RAW;
                            if (ISTFP)
                                idxx += CubieCube.FlipS2RF[CubieCube.FlipR2S[rawx] ^ j];
                            else
                                idxx += RawConj[rawx, j ^ (SYM_E2C_MAGIC >> (j << 1) & 3)];

                            if (getPruning(PrunTable, idxx) == check) {
                                setPruning(PrunTable, idxx, xorVal);
                                done++;
                            }
                        }
                    }
                }
            }

            return Search.PARTIAL_INIT_LEVEL > 1 || depth == MAX_DEPTH;
        }

        static bool initTwistFlipPrun() => initRawSymPrun(
                       TwistFlipPrun,
                       null, null,
                       TwistMove, CubieCube.SymStateTwist, 0x19603
                   );

        static bool initSliceTwistPrun() => initRawSymPrun(
                       UDSliceTwistPrun,
                       UDSliceMove, UDSliceConj,
                       TwistMove, CubieCube.SymStateTwist, 0x69603
                   );

        static bool initSliceFlipPrun() => initRawSymPrun(
                       UDSliceFlipPrun,
                       UDSliceMove, UDSliceConj,
                       FlipMove, CubieCube.SymStateFlip, 0x69603
                   );

        static bool initMEPermPrun() => initRawSymPrun(
                       MEPermPrun,
                       MPermMove, MPermConj,
                       EPermMove, CubieCube.SymStatePerm, 0x7c724
                   );

        static bool initMCPermPrun() => initRawSymPrun(
                       MCPermPrun,
                       MPermMove, MPermConj,
                       CPermMove, CubieCube.SymStatePerm, 0x8ea34
                   );

        static bool initPermCombPrun() => initRawSymPrun(
                       EPermCCombPrun,
                       CCombMove, CCombConj,
                       EPermMove, CubieCube.SymStatePerm, 0x7c824
                   );

        public int twist;
        public int tsym;
        public int flip;
        public int fsym;
        public int slice;
        public int prun;

        public int twistc;
        public int flipc;

        public CoordCube() { }

        public void set(CoordCube node) {
            twist = node.twist;
            tsym = node.tsym;
            flip = node.flip;
            fsym = node.fsym;
            slice = node.slice;
            prun = node.prun;

            if (Search.USE_CONJ_PRUN) {
                twistc = node.twistc;
                flipc = node.flipc;
            }
        }

        public void calcPruning(bool isPhase1) {
            prun = Math.Max(
                       Math.Max(
                           getPruning(UDSliceTwistPrun,
                                      twist * N_SLICE + UDSliceConj[slice & 0x1ff, tsym]),
                           getPruning(UDSliceFlipPrun,
                                      flip * N_SLICE + UDSliceConj[slice & 0x1ff, fsym])),
                       Math.Max(
                           Search.USE_CONJ_PRUN ? getPruning(TwistFlipPrun,
                                   (twistc >> 3) << 11 | CubieCube.FlipS2RF[flipc ^ (twistc & 7)]) : 0,
                           Search.USE_TWIST_FLIP_PRUN ? getPruning(TwistFlipPrun,
                                   twist << 11 | CubieCube.FlipS2RF[flip << 3 | (fsym ^ tsym)]) : 0));
        }

        public void set(CubieCube cc) {
            twist = cc.getTwistSym();
            flip = cc.getFlipSym();
            slice = cc.getUDSlice();
            tsym = twist & 7;
            twist = twist >> 3;
            fsym = flip & 7;
            flip = flip >> 3;

            if (Search.USE_CONJ_PRUN) {
                CubieCube pc = new CubieCube();
                CubieCube.CornConjugate(cc, 1, pc);
                CubieCube.EdgeConjugate(cc, 1, pc);
                twistc = pc.getTwistSym();
                flipc = pc.getFlipSym();
            }
        }

        /*
        *      return
        *      0: Success
        *      1: Try Next Power
        *      2: Try Next Axis
        */
        public int doMovePrun(CoordCube cc, int m, bool isPhase1) {
            slice = UDSliceMove[cc.slice & 0x1ff, m] & 0x1ff;

            flip = FlipMove[cc.flip, CubieCube.Sym8Move[m << 3 | cc.fsym]];
            fsym = (flip & 7) ^ cc.fsym;
            flip >>= 3;

            twist = TwistMove[cc.twist, CubieCube.Sym8Move[m << 3 | cc.tsym]];
            tsym = (twist & 7) ^ cc.tsym;
            twist >>= 3;

            prun = Math.Max(
                       Math.Max(
                           getPruning(UDSliceTwistPrun,
                                      twist * N_SLICE + UDSliceConj[slice, tsym]),
                           getPruning(UDSliceFlipPrun,
                                      flip * N_SLICE + UDSliceConj[slice, fsym])),
                       Search.USE_TWIST_FLIP_PRUN ? getPruning(TwistFlipPrun,
                               twist << 11 | CubieCube.FlipS2RF[flip << 3 | (fsym ^ tsym)]) : 0);
            return prun;
        }

        public int doMovePrunConj(CoordCube cc, int m) {
            m = CubieCube.SymMove[3, m];
            flipc = FlipMove[cc.flipc >> 3, CubieCube.Sym8Move[m << 3 | cc.flipc & 7]] ^ (cc.flipc & 7);
            twistc = TwistMove[cc.twistc >> 3, CubieCube.Sym8Move[m << 3 | cc.twistc & 7]] ^ (cc.twistc & 7);

            return getPruning(TwistFlipPrun,
                              (twistc >> 3) << 11 | CubieCube.FlipS2RF[flipc ^ (twistc & 7)]);
        }
    }
}
