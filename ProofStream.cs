using System; 
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ProofStream
{
    class Program
    {
        static void Main(string[] args)
        {
            string h = "Usage: ProofStream {0} {1}{2} <number_of_steps> <number_of_proovers>{3}";
            if (args.Length > 1 && byte.TryParse(args[0], out var byteDet) && byte.TryParse(args[1], out var bytePrf))
            {
                bool bDet = BitConverter.ToBoolean(new byte[] { byteDet }, 0);
                bool bPrf = BitConverter.ToBoolean(new byte[] { bytePrf }, 0);
                h = string.Format(h, byteDet, bytePrf, bDet && bPrf ? "" : " <number_of_blocks>", bDet ? "" : " <number_of_positions>");
                if (bDet)
                {
                    if (bPrf)
                    {
                        if (args.Length == 4 && int.TryParse(args[2], out var nSteps) && int.TryParse(args[3], out var nProvers) && nSteps > 0 && nProvers > 0)
                            DP(nSteps, nProvers);
                        else
                            Console.WriteLine(h);
                    }
                    else
                    {
                        if (args.Length == 5 && int.TryParse(args[2], out var nBlocks) && int.TryParse(args[3], out var nSteps) && int.TryParse(args[4], out var nProvers) &&
                                nBlocks > 0 && nSteps > 0 && nProvers > 0)
                            DS(nBlocks, nSteps, nProvers);
                        else
                            Console.WriteLine(h);
                    }
                }
                else
                {
                    if (args.Length == 6 && int.TryParse(args[2], out var nBlocks) && int.TryParse(args[3], out var nSteps) && int.TryParse(args[4], out var nProvers) &&
                    nBlocks > 0 && nSteps > 0 && nProvers > 0 && int.TryParse(args[5], out var nPositions) && nPositions > 0)
                    {
                        if (bPrf)
                            SP(nBlocks, nSteps, nProvers, nPositions);
                        else
                            SS(nBlocks, nSteps, nProvers, nPositions);
                    }
                    else if (args.Length == 9 && int.TryParse(args[2], out var nItr) && int.TryParse(args[3], out var nBl) && int.TryParse(args[4], out var nSt) && int.TryParse(args[5], out var nPr) &&
                    nItr > 0 && nBl > 0 && nSt > 0 && nPr > 0 && int.TryParse(args[6], out var nPosMin) && int.TryParse(args[7], out var nPosMax) && int.TryParse(args[8], out var nPosStep) && nPosMin > 0 && nPosMax > 0 && nPosStep > 0)
                    {
                        if (bPrf)
                            SPP(nItr, nBl, nSt, nPr, nPosMin, nPosMax, nPosStep);
                        else
                            SSP(nItr, nBl, nSt, nPr, nPosMin, nPosMax, nPosStep);
                    }
                    else
                        Console.WriteLine(h);
                }
            }
            else
                Console.WriteLine(string.Format(h, "<1/0:deterministic/stochastic>", "<1/0: perfect/strict>", " [<number_of_blocks>]", " [<number_of_positions>]"));
        }

        static void DS(int nBlocks, int nSteps, int nProvers)
        {
            int pBuffer = 0;
            List<int> L = new List<int>();
            List<int> H = new List<int>();
            while (pBuffer < nBlocks)
            {
                for (int j = 0; j < nSteps; j++)
                {
                    for (int k = L.Count; k < pBuffer + 2 * nProvers; k++)
                    {
                        L.Add(1);
                        H.Add(0);
                    }
                    for (int k = pBuffer + 2 * (nProvers - 1); k >= pBuffer; k -= 2)
                    {
                        L[k] += L[k + 1];
                        L.RemoveAt(k + 1);
                        H[k]++;
                        H.RemoveAt(k + 1);
                    }
                }
                pBuffer++;
            }
            Console.WriteLine("Published:");
            for (int i = 0; i < pBuffer; i++)
                Console.Write("{0}/{1} ", L[i], H[i]);
            Console.WriteLine();
            Console.WriteLine("Buffer:");
            for (int i = pBuffer; i < L.Count; i++)
                Console.Write("{0}/{1} ", L[i], H[i]);
        }

        static void SS(int nBlocks, int nSteps, int nProvers, int nPositions)
        {
            int pBuffer = 0;
            List<int> L = new List<int>();
            List<int> H = new List<int>();
            while (pBuffer < nBlocks)
            {
                for (int j = 0; j < nSteps; j++)
                {
                    var intBytes = new byte[4];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(intBytes);
                    }
                    var random = new Random(BitConverter.ToInt32(intBytes, 0));
                    var ss = new SortedSet<int>();
                    for (int k = 0; k < nProvers; k++)
                        ss.Add(random.Next(nPositions));
                    for (int k = L.Count; k < pBuffer + 2 * (ss.Max() + 1); k++)
                    {
                        L.Add(1);
                        H.Add(0);
                    }
                    foreach (int kk in ss.Reverse())
                    {
                        int k = pBuffer + 2 * kk;
                        L[k] += L[k + 1];
                        L.RemoveAt(k + 1);
                        H[k] = Math.Max(H[k],H[k+1]);
                        H[k]++;
                        H.RemoveAt(k + 1);
                    }
                }
                pBuffer++;
            }
            int Eff = 0;
            for (int i = 0; i < pBuffer; i++)
            {
                Eff += L[i];
                Eff--;
                Console.Write("{0}/{1} ", L[i], H[i]);
            }
            Console.WriteLine();
            Console.WriteLine("Buffer:");
            int Prd = Eff;
            for (int i = pBuffer; i < L.Count; i++)
            {
                Prd += L[i];
                Prd--;
                Console.Write("{0}/{1} ", L[i], H[i]);
            }
            Console.WriteLine("Efficiency/Productivity:");
            int n = nBlocks * nSteps * nProvers;
            Console.WriteLine("{0} / {1}", (double)Eff / n, (double)Prd / n);
        }

        static void SSP(int nItr, int nBlocks, int nSteps, int nProvers, int nPosMin, int nPosMax, int nPosStep)
        {
            long n1 = nItr * nBlocks * nSteps * nProvers;
            long n2 = n1 * n1;
            var cltr = new CultureInfo("", false);
            StringBuilder sb1 = new StringBuilder("ListPlot[{");
            StringBuilder sb2 = new StringBuilder("ListPlot[{");
            bool sb = false;
            for (int nPositions = nPosMin; nPositions <= nPosMax; nPositions += nPosStep)
            {
                long M1 = 0;
                long M2 = 0;
                for (int itr = 0; itr < nItr; itr++)
                {
                    int pBuffer = 0;
                    List<int> L = new List<int>();
                    while (pBuffer < nBlocks)
                    {
                        for (int j = 0; j < nSteps; j++)
                        {
                            var intBytes = new byte[4];
                            using (var rng = new RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(intBytes);
                            }
                            var random = new Random(BitConverter.ToInt32(intBytes, 0));
                            var ss = new SortedSet<int>();
                            for (int k = 0; k < nProvers; k++)
                                ss.Add(random.Next(nPositions));
                            for (int k = L.Count; k < pBuffer + 2 * (ss.Max() + 1); k++)
                                L.Add(1);
                            foreach (int kk in ss.Reverse())
                            {
                                int k = pBuffer + 2 * kk;
                                L[k] += L[k + 1];
                                L.RemoveAt(k + 1);
                            }
                        }
                        pBuffer++;
                    }
                    int Eff = 0;
                    for (int i = 0; i < pBuffer; i++)
                        Eff += L[i];
                    Eff -= pBuffer;
                    M1 += Eff;
                    M2 += Eff * Eff;
                }
                if (sb)
                {
                    sb1.Append(',');
                    sb2.Append(',');
                }
                else
                    sb = true;
                double ex = (double)M1 / n1;
                sb1.Append(String.Format(cltr, "{{{0},{1}}}", nPositions, ex));
                sb2.Append(String.Format(cltr, "{{{0},Around[{1},{2}]}}", nPositions, ex, Math.Sqrt((double)(nItr * M2 - M1 * M1) / n2)));
            }
            sb1.Append("}]");
            sb2.Append("}]");
            Console.WriteLine(sb1.ToString());
            Console.WriteLine("(* nItr={0}, nBlocks={1}, nSteps={2}, nProvers={3} *)", nItr, nBlocks, nSteps, nProvers);
            Console.WriteLine(sb2.ToString());
        }

        static void DP(int k, int m)
        {
            int lmax = (int)Math.Log(k * m, 2) + 1;
            int[] mu = new int[lmax];
            List<int[]> Mu = new List<int[]>();
            List<int> A = new List<int>();
            int cMu = -1;
            Console.WriteLine("Buffer in each step and published blocks (before the period is found)");
            while (cMu == -1)
            {
                Mu.Add((int[])mu.Clone());
                foreach (var p in mu)
                {
                    Console.Write(p);
                    Console.Write(' ');
                }
                Console.Write(@"/ ");
                for (int kk = 0; kk < k; kk++)
                {
                    int mm = m;
                    for (int l = 0; l < lmax; l++)
                    {
                        int dm = mu[l] / 2;
                        if (dm > 0)
                        {
                            mm -= dm;
                            mu[l - 1] += dm;
                            mu[l] -= 2 * dm;
                        }
                    }
                    mu[lmax - 1] += mm;
                    foreach (var p in mu)
                    {
                        Console.Write(p);
                        Console.Write(' ');
                    }
                    Console.Write(@"/ ");
                }
                for (int l = 0; l < lmax; l++)
                {
                    if (mu[l] > 0)
                    {
                        A.Add(lmax - l);
                        Console.WriteLine(lmax - l);
                        mu[l]--;
                        break;
                    }
                }
                cMu++;
                while (cMu < Mu.Count)
                {
                    var mu1 = Mu[cMu];
                    bool eq = true;
                    for (int l = 0; l < lmax; l++)
                    {
                        if (mu[l] != mu1[l])
                        {
                            eq = false;
                            break;
                        }
                    }
                    if (eq)
                        break;
                    cMu++;
                }
                if (cMu == Mu.Count)
                    cMu = -1;
            }

            Console.WriteLine("Upper bounds on trees in buffer after block publishing");
            int[] mumax = new int[lmax];
            foreach (var mu1 in Mu)
                for (int l = 0; l < lmax; l++)
                    if (mu1[l] > mumax[l])
                        mumax[l] = mu1[l];
            for (int l = 0; l < lmax; l++)
            {
                Console.Write(mumax[l]);
                Console.Write(' ');
            }
            Console.WriteLine();

            Console.WriteLine("Pre-period(period)");
            DPprint(A, 0, cMu);
            if (cMu > 0)
                Console.Write(',');
            Console.Write('(');
            DPprint(A, cMu, A.Count);
            Console.WriteLine(')');
            Console.WriteLine("Pre-period length(period length)");
            Console.Write(cMu);
            Console.Write("(2^");
            int pl = A.Count - cMu;
            int pow = 0;
            while (pl % 2 == 0)
            {
                pl /= 2;
                pow++;
            }
            Console.Write(pow);
            if (pl > 1)
            {
                Console.Write('*');
                Console.Write(pl);
            }
            Console.WriteLine(')');
        }

        static void DPprint(List<int> A, int i1, int i2)
        {
            int aa = 0;
            int ii = 1;
            for (int i = i1; i < i2; i++)
            {
                int a = A[i];
                if (a == aa)
                    ii++;
                else
                {
                    if (aa > 0)
                        Console.Write(aa);
                    aa = a;
                    if (ii > 1)
                    {
                        Console.Write('^');
                        Console.Write(ii);
                    }
                    if (i != i1)
                        Console.Write(',');
                    if (aa > 0)
                        ii = 1;
                }
            }
            if (aa > 0)
                Console.Write(aa);
            if (ii > 1)
            {
                Console.Write('^');
                Console.Write(ii);
            }
        }

        static void SP(int nBlocks, int nSteps, int nProvers, int nPositions)
        {
            List<int> Bufer = new List<int>();
            List<int> Pub = new List<int>();
            int PubNodes = 0;
            int maxBuferNTrees = 0;
            int maxBuferNTrans = 0;
            for (int i = 0; i < nBlocks; i++)
            {
                for (int j = 0; j < nSteps; j++)
                {
                    int countTr = 0;
                    List<int> MPairs = new List<int>();
                    for (int k = 0; k < Bufer.Count() - 1; k++)
                    {
                        if (MPairs.Count == nPositions)
                            break;
                        if (Bufer[k] == Bufer[k + 1] && countTr == countTr >> (Bufer[k] + 1) << (Bufer[k] + 1))
                            MPairs.Add(k);
                        countTr += 1 << Bufer[k];
                    }
                    if (nPositions > MPairs.Count())
                    {
                        int addPos = nPositions - MPairs.Count();
                        for (int k = 0; k < addPos; k++)
                        {
                            MPairs.Add(Bufer.Count());
                            Bufer.Add(0);
                            Bufer.Add(0);
                        }
                    }
                    var intBytes = new byte[4];
                    using (var rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(intBytes);
                    }
                    var random = new Random(BitConverter.ToInt32(intBytes, 0));
                    var ss = new SortedSet<int>();
                    for (int k = 0; k < nProvers; k++)
                        ss.Add(random.Next(nPositions));
                    foreach (var s in ss.Reverse())
                    {
                        Bufer[MPairs[s]] += 1;
                        Bufer.RemoveAt(MPairs[s] + 1);
                    }
                    while (Bufer[Bufer.Count() - 1] == 0)
                        Bufer.RemoveAt(Bufer.Count() - 1);
                }
                int l = Bufer[0];
                Pub.Add(l);
                Bufer.RemoveAt(0);
                PubNodes += (1 << l) - 1;
                if (Bufer.Count() > maxBuferNTrees)
                    maxBuferNTrees = Bufer.Count();
                int BuferNTrans = 0;
                foreach (var x in Bufer)
                    BuferNTrans += 1 << x;
                if (BuferNTrans > maxBuferNTrans)
                    maxBuferNTrans = BuferNTrans;
                // foreach (var x in Bufer) { Console.Write(x); Console.Write(' '); } Console.WriteLine();
            }

            long Eff = 0;
            foreach (var x in Pub)
                Eff += 1 << x;
            Eff -= Pub.Count;
            long Prd = Eff;
            foreach (var x in Bufer)
                Prd += 1 << x;
            Prd -= Bufer.Count;

            //Console.WriteLine("Published:"); foreach (var x in Pub) { Console.Write(x); Console.Write(' '); } Console.WriteLine();

            Console.WriteLine("Average number of levels in published trees \t {0} / {1}", Math.Log((double)PubNodes / nBlocks, 2), Math.Log((double)nProvers * nSteps + 1, 2));
            Console.WriteLine("Max trees, transaction number in bufer \t\t {0}, {1}", maxBuferNTrees, maxBuferNTrans);

            Console.WriteLine("Efficiency/Productivity:");
            int n = nBlocks * nSteps * nProvers;
            Console.WriteLine("{0} / {1}", (double)Eff / n, (double)Prd / n);
        }

        static void SPP(int nItr, int nBlocks, int nSteps, int nProvers, int nPosMin, int nPosMax, int nPosStep)
        {
            long n1 = nItr * nBlocks * nSteps * nProvers;
            long n2 = n1 * n1;
            var cltr = new CultureInfo("", false);
            StringBuilder sb1 = new StringBuilder("ListPlot[{");
            StringBuilder sb2 = new StringBuilder("ListPlot[{");
            bool sb = false;
            for (int nPositions = nPosMin; nPositions <= nPosMax; nPositions += nPosStep)
            {
                long M1 = 0;
                long M2 = 0;
                for (int itr = 0; itr < nItr; itr++)
                {
                    List<int> Bufer = new List<int>();
                    List<int> Pub = new List<int>();
                    int PubNodes = 0;
                    int maxBuferNTrees = 0;
                    int maxBuferNTrans = 0;
                    for (int i = 0; i < nBlocks; i++)
                    {
                        for (int j = 0; j < nSteps; j++)
                        {
                            int countTr = 0;
                            List<int> MPairs = new List<int>();
                            for (int k = 0; k < Bufer.Count() - 1; k++)
                            {
                                if (MPairs.Count == nPositions)
                                    break;
                                if (Bufer[k] == Bufer[k + 1] && countTr == countTr >> (Bufer[k] + 1) << (Bufer[k] + 1))
                                    MPairs.Add(k);
                                countTr += 1 << Bufer[k];
                            }
                            if (nPositions > MPairs.Count())
                            {
                                int addPos = nPositions - MPairs.Count();
                                for (int k = 0; k < addPos; k++)
                                {
                                    MPairs.Add(Bufer.Count());
                                    Bufer.Add(0);
                                    Bufer.Add(0);
                                }
                            }
                            var intBytes = new byte[4];
                            using (var rng = new RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(intBytes);
                            }
                            var random = new Random(BitConverter.ToInt32(intBytes, 0));
                            var ss = new SortedSet<int>();
                            for (int k = 0; k < nProvers; k++)
                                ss.Add(random.Next(nPositions));
                            foreach (var s in ss.Reverse())
                            {
                                Bufer[MPairs[s]] += 1;
                                Bufer.RemoveAt(MPairs[s] + 1);
                            }
                            while (Bufer[Bufer.Count() - 1] == 0)
                                Bufer.RemoveAt(Bufer.Count() - 1);
                        }
                        int l = Bufer[0];
                        Pub.Add(l);
                        Bufer.RemoveAt(0);
                        PubNodes += (1 << l) - 1;
                        if (Bufer.Count() > maxBuferNTrees)
                            maxBuferNTrees = Bufer.Count();
                        int BuferNTrans = 0;
                        foreach (var x in Bufer)
                            BuferNTrans += 1 << x;
                        if (BuferNTrans > maxBuferNTrans)
                            maxBuferNTrans = BuferNTrans;
                        // foreach (var x in Bufer) { Console.Write(x); Console.Write(' '); } Console.WriteLine();
                    }

                    int Eff = 0;
                    foreach (var x in Pub)
                        Eff += 1 << x;
                    Eff -= nBlocks;
                    M1 += Eff;
                    M2 += Eff * Eff;
                }
                if (sb)
                {
                    sb1.Append(',');
                    sb2.Append(',');
                }
                else
                    sb = true;
                double ex = (double)M1 / n1;
                sb1.Append(String.Format(cltr, "{{{0},{1}}}", nPositions, ex));
                sb2.Append(String.Format(cltr, "{{{0},Around[{1},{2}]}}", nPositions, ex, Math.Sqrt((double)(nItr * M2 - M1 * M1) / n2)));
            }
            sb1.Append("}]");
            sb2.Append("}]");
            Console.WriteLine(sb1.ToString());
            Console.WriteLine("(* nItr={0}, nBlocks={1}, nSteps={2}, nProvers={3} *)", nItr, nBlocks, nSteps, nProvers);
            Console.WriteLine(sb2.ToString());
        }

    }
}

