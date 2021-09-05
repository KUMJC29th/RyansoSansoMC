using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyansoSansoMC
{
    public class Worker
    {
        private readonly Random random;

        public Worker(int seed)
        {
            this.random = new Random(seed);
        }

        private Span<int> CreateHand()
        {
            int[] hand = new int[14];
            for (int i = 0; i < hand.Length; ++i)
            {
                hand[i] = this.random.Next(0, 136) / 4;
            }
            return hand.AsSpan();
        }

        public (int canRiichi, int riichiTileIsRyanso, int winTileIsSanso, int winTileIsHonors) Run(int numOfTrials)
        {
            int canRiichi = 0;
            int riichiTileIsRyanso = 0;
            int winTileIsSanso = 0;
            int winTileIsHonors = 0;
            for (int i = 0; i < numOfTrials; ++i)
            {
                var hand14 = this.CreateHand();
                var tempaiInfos = TempaiInfo.GetBestTempaiInfo(hand14);
                if (tempaiInfos.Count > 0)
                {
                    ++canRiichi;
                    int index = tempaiInfos.Count == 1 ? 0 : this.random.Next(tempaiInfos.Count);
                    if (tempaiInfos[index].Discard == 20)
                    {
                        ++riichiTileIsRyanso;
                        if ((tempaiInfos[index].WinTiles & 1UL << 21) > 0)
                        {
                            ++winTileIsSanso;
                        }
                        if ((tempaiInfos[index].WinTiles & 0b1111111UL << 27) > 0)
                        {
                            ++winTileIsHonors;
                        }
                    }
                }
            }
            return (canRiichi, riichiTileIsRyanso, winTileIsSanso, winTileIsHonors);
        }
    }
}
