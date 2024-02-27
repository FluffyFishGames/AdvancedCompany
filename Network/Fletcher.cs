using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Network
{
    internal static class Fletcher
    {
        static IEnumerable<ulong> Blockify(IReadOnlyList<byte> inputAsBytes, int blockSize)
        {
            var i = 0;
            ulong block = 0;

            while (i < inputAsBytes.Count)
            {
                block = (block << 8) | inputAsBytes[i];
                i++;

                if (i % blockSize != 0 && i != inputAsBytes.Count) continue;

                yield return block;
                block = 0;
            }
        }

        static byte[] GetFletcher(IReadOnlyList<byte> input, int n = 32) // Fletcher 32, 16, 64
        {
            var bytesPerCycle = n / 16;
            var modValue = (ulong)(System.Math.Pow(2, 8 * bytesPerCycle) - 1);

            ulong sum1 = 0;
            ulong sum2 = 0;

            foreach (var block in Blockify(input, bytesPerCycle))
            {
                sum1 = (sum1 + block) % modValue;
                sum2 = (sum2 + sum1) % modValue;
            }

            return BitConverter.GetBytes(sum1 + sum2 * (modValue + 1));
        }
    }
}
