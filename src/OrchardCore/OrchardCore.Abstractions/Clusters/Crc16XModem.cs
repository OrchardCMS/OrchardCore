using System.Text;

namespace OrchardCore.Clusters;

/// <summary>
/// Implements the 'CRC-16/XMODEM' algorithm.
/// </summary>
public class Crc16XModem
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
    /// Checks the 'CRC-16' algorithm.
    /// </summary>
    public static bool Check() => Compute("123456789") == 0x31c3;

    /// <summary>
    /// Computes the 'CRC-16' of the provided string.
    /// </summary>
    public static int Compute(string input) => Compute(Encoding.ASCII.GetBytes(input));


    /// <summary>
    /// Computes the 'CRC-16' of the provided byte array.
    /// </summary>
    public static int Compute(byte[] input)
    {
        var result = 0;
        for (var i = 0; i < input.Length; i++)
        {
            result = _lookup[(result >> 8 ^ input[i]) & 0xff] ^ result << 8;
        }

        return result & 0xffff;
    }
}
