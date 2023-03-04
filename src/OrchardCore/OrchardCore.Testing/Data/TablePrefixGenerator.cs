using System;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Testing.Data
{
    /// <summary>
    /// This is an internal table prefix generator which uses a timestamp to generate a table prefix
    /// is unique during a test run and within the character limits of the supported sql databases.
    /// </summary>
    /// <remarks>
    /// Does not guarantee uniqueness.
    /// </remarks>
    internal class TablePrefixGenerator
    {
        private static readonly char[] _charList = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        public async Task<string> GeneratePrefixAsync()
        {
            await Task.Delay(1);
            var ticks = DateTime.Now.Ticks;

            var result = new StringBuilder();
            while (ticks != 0)
            {
                result.Append(_charList[ticks % _charList.Length]);

                ticks /= _charList.Length;
            }

            return result.ToString();
        }
    }
}
