using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyansoSansoMC
{
    public readonly struct TempaiInfo
    {
        public int Discard { get; }
        public ulong WinTiles { get; }
        public int KindsCount { get; }
        public int TilesCount { get; }

        public TempaiInfo(int discard, ulong winTiles, int kindsCount, int tilesCount)
        {
            this.Discard = discard;
            this.WinTiles = winTiles;
            this.KindsCount = kindsCount;
            this.TilesCount = tilesCount;
        }

        public static List<TempaiInfo> GetBestTempaiInfo(Span<int> hand14)
        {
            static int[] convertToTilesMap(Span<int> hand)
            {
                int[] tilesMap = new int[34];
                for (int i = 0; i < hand.Length; ++i)
                {
                    ++tilesMap[hand[i]];
                }
                return tilesMap;
            }

            static bool canReduceWithoutHead(Span<int> tilesMap)
            {
                int r;
                int a = tilesMap[0];
                int b = tilesMap[1];

                for (int i = 0; i < 7; ++i)
                {
                    r = a % 3;
                    if (b >= r && tilesMap[i + 2] >= r)
                    {
                        a = b - r;
                        b = tilesMap[i + 2] - r;
                    }
                    else
                    {
                        return false;
                    }
                }

                return a % 3 == 0 && b % 3 == 0;
            }

            static int sumTileNumbers(Span<int> tilesMap)
            {
                int acc = 0;
                for (int i = 0; i < tilesMap.Length; ++i)
                {
                    acc += i * tilesMap[i];
                }
                return acc;
            }

            static bool canReduceWithHead(Span<int> tilesMap)
            {
                int tileNumbersSum = sumTileNumbers(tilesMap);

                // tileNumbersSumを3で割った余りが
                // 0 -> 雀頭は3,6,9のいずれか
                // 1 -> 雀頭は2,5,8のいずれか
                // 2 -> 雀頭は1,4,7のいずれか
                for (int i = tileNumbersSum * 2 % 3; i < 9; i += 3)
                {
                    if (tilesMap[i] >= 2)
                    {
                        // 雀頭を取り除いて面子分解を試みる
                        tilesMap[i] -= 2;
                        if (canReduceWithoutHead(tilesMap))
                        {
                            tilesMap[i] += 2;
                            return true;
                        }
                        tilesMap[i] += 2;
                    }
                }

                return false;
            }

            static bool isRegularCompletedHand(Span<int> tilesMap)
            {
                // どの種類の牌が雀頭になるか
                // -1: 未確定、0: 萬子、1: 筒子、2: 索子、27-33: 字牌
                int head = -1;

                // 数牌に雀頭があるかどうかを探索する
                for (int i = 0; i < 3; ++i)
                {
                    switch (sumTileNumbers(tilesMap[(9 * i)..(9 * i + 9)]) % 3)
                    {
                        case 1:
                            return false;
                        case 2:
                            {
                                if (head == -1)
                                {
                                    head = i;
                                    break;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        default:
                            break;
                    }
                }
                // 字牌に雀頭があるかどうかを探索する
                for (int i = 27; i < 34; ++i)
                {
                    switch (tilesMap[i] % 3)
                    {
                        case 1:
                            return false;
                        case 2:
                            {
                                if (head == -1)
                                {
                                    head = i;
                                    break;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        default:
                            break;
                    }
                }
                // 数牌の面子分解を試みる
                for (int i = 0; i < 3; ++i)
                {
                    if (head == i)
                    {
                        if (!canReduceWithHead(tilesMap[(9 * i)..(9 * i + 9)]))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!canReduceWithoutHead(tilesMap[(9 * i)..(9 * i + 9)]))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            static bool isSevenPairsCompletedHand(Span<int> tilesMap)
            {
                int pairsCount = 0;
                for (int i = 0; i < tilesMap.Length; ++i)
                {
                    switch (tilesMap[i])
                    {
                        case 0:
                            break;
                        case 2:
                            {
                                ++pairsCount;
                                break;
                            }
                        default:
                            return false;
                    }
                }
                return pairsCount == 7;
            }

            static bool isThirteenOrphansCompletedHand(Span<int> tilesMap)
            {
                return tilesMap[0] > 0
                    && tilesMap[8] > 0
                    && tilesMap[9] > 0
                    && tilesMap[17] > 0
                    && tilesMap[18] > 0
                    && tilesMap[26] > 0
                    && tilesMap[27] > 0
                    && tilesMap[28] > 0
                    && tilesMap[29] > 0
                    && tilesMap[30] > 0
                    && tilesMap[31] > 0
                    && tilesMap[32] > 0
                    && tilesMap[33] > 0;
            }

            static bool isCompletedHand(Span<int> tilesMap)
            {
                return isSevenPairsCompletedHand(tilesMap) || isThirteenOrphansCompletedHand(tilesMap) || isRegularCompletedHand(tilesMap);
            }

            static int compareTempaiInfo(ulong winTiles, int kindsCount, int tilesCount, TempaiInfo other)
            {
                // 待ち枚数が多い方が良い
                int tilesCountDif = tilesCount - other.TilesCount;
                if (tilesCountDif != 0) return tilesCountDif;

                // 待ち枚数が同じならば、待ちの種類が多い方が良い
                int kindsCountDif = kindsCount - other.KindsCount;
                if (kindsCountDif != 0) return kindsCountDif;

                // 待ちの種類も同数ならば、字牌を含む方が良い
                // 双方待ちに字牌を含まないならば、端牌を含むほうが良い
                int xOrphans = (winTiles & 0b1111111UL << 27) > 0 ? 2
                    : (winTiles & 0b100000001100000001100000001UL) > 0 ? 1 : 0;
                int yOrphans = (other.WinTiles & 0b1111111UL << 27) > 0 ? 2
                    : (other.WinTiles & 0b100000001100000001100000001UL) > 0 ? 1 : 0;
                return xOrphans - yOrphans;
            }

            List<TempaiInfo> tempaiInfos = new();
            Span<int> tilesMap = convertToTilesMap(hand14);
            for (int i = 0; i < tilesMap.Length; ++i)
            {
                // 1枚以上持っている牌について
                if (tilesMap[i] > 0)
                {
                    // その牌を1枚減らす
                    --tilesMap[i];

                    ulong winTiles = 0;
                    int kindsCount = 0;
                    int tilesCount = 0;
                    for (int j = 0; j < tilesMap.Length; ++j)
                    {
                        // 減らした牌と4枚使いの牌以外の牌を順に1枚ずつ足して和了形になっているかどうかを見る
                        if (j == i || tilesMap[j] == 4) continue;
                        ++tilesMap[j];
                        if (isCompletedHand(tilesMap))
                        {
                            winTiles |= 1UL << j;
                            ++kindsCount;
                            tilesCount += 5 - tilesMap[j];
                        }
                        // 増やした牌を戻して次へ
                        --tilesMap[j];
                    }
                    if (winTiles > 0)
                    {
                        // どの牌を切って聴牌するのがよいのかを判断
                        if (tempaiInfos.Count == 0)
                        {
                            tempaiInfos.Add(new TempaiInfo(i, winTiles, kindsCount, tilesCount));
                        }
                        else
                        {
                            int comp = compareTempaiInfo(winTiles, kindsCount, tilesCount, tempaiInfos[0]);
                            if (comp > 0)
                            {
                                tempaiInfos = new List<TempaiInfo>() { new TempaiInfo(i, winTiles, kindsCount, tilesCount) };
                            }
                            else if (comp == 0)
                            {
                                tempaiInfos.Add(new TempaiInfo(i, winTiles, kindsCount, tilesCount));
                            }
                        }
                    }

                    // 減らした牌を戻して次へ
                    ++tilesMap[i];
                }
            }

            return tempaiInfos;
        }
    }
}
