using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RyansoSansoMC
{
    class Program
    {
        static void Main(string[] args)
        {
            int N = 100_000;
            Random random = new();
            var lockObject = new object();
            decimal numOfTrials = 0;
            decimal canRiichi = 0;
            decimal riichiTileIsRyanso = 0;
            decimal winTileIsSanso = 0;
            decimal winTileIsHonors = 0;

            using var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Console.WriteLine("計算開始");
            Console.WriteLine("いずれかのキーを押すと終了します。");
            Console.WriteLine();

            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < 4; ++i)
            {
                Task.Run(() =>
                    {
                        var worker = new Worker(random.Next());
                        while (true)
                        {
                            var result = worker.Run(N);
                            lock (lockObject)
                            {
                                numOfTrials += N;
                                canRiichi += result.canRiichi;
                                riichiTileIsRyanso += result.riichiTileIsRyanso;
                                winTileIsSanso += result.winTileIsSanso;
                                winTileIsHonors += result.winTileIsHonors;
                                Console.WriteLine($"試行回数: {numOfTrials}");
                                Console.WriteLine($"リーチ可能:  {canRiichi}");
                                Console.WriteLine($"2s切りリーチ: {riichiTileIsRyanso}");
                                Console.WriteLine($"3sが当たり牌: {winTileIsSanso} ({(riichiTileIsRyanso == 0 ? null : winTileIsSanso / riichiTileIsRyanso):P2})");
                                Console.WriteLine($"字牌が当たり牌: {winTileIsHonors} ({(riichiTileIsRyanso == 0 ? null : winTileIsHonors / riichiTileIsRyanso):P2})");
                                Console.WriteLine();
                            }

                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                        }
                    }
                );
            }

            Console.ReadKey();
            cancellationTokenSource.Cancel();
            sw.Stop();
            Console.WriteLine();
            Console.WriteLine($"経過時間: {sw.Elapsed.Hours}:{sw.Elapsed.Minutes:D2}:{sw.Elapsed.Seconds:D2} ({(numOfTrials / sw.ElapsedMilliseconds * 1000):F0}試行/秒)");
        }
    }
}
