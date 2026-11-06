using System.Text;

namespace OrchardCore.Clusters;

/// <summary>
/// Implements the 'CRC-16/XMODEM' algorithm.
/// </summary>
public static class Crc16XModem
{
    private static readonly int[] _lookup = new int[256];

    static Crc16XModem()
    {
        for (var i = 0; i < _lookup.Length; i++)
        {
            var result = i << 8;
            for (var n = 0; n < 8; n++)
            {
                result <<= 1;
                if ((result & 0x10000) != 0)
                {
                    result ^= 0x1021;
                }
            }

            _lookup[i] = result;
        }
    }

    /// <summary>
    /// Computes the 'CRC-16' of the provided string.
    /// </summary>
    public static int Compute(string input)
    {
        var result = 0;
        foreach (var value in Encoding.ASCII.GetBytes(input))
        {
            result = _lookup[(result >> 8 ^ value) & 0xff] ^ result << 8;
        }

        return result & 0xffff;
    }
}
