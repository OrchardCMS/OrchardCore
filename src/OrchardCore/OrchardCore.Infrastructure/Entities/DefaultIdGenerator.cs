using System;

namespace OrchardCore.Entities
{
    public class DefaultIdGenerator : IIdGenerator
    {
        // Some confusing chars are ignored: http://www.crockford.com/wrmg/base32.html
        private const string Encode32Chars = "0123456789abcdefghjkmnpqrstvwxyz";

        public string GenerateUniqueId()
        {
            // Generate a base32 Guid value
            var guid = Guid.NewGuid().ToByteArray();

            var hs = BitConverter.ToInt64(guid, 0);
            var ls = BitConverter.ToInt64(guid, 8);

            return ToBase32(hs, ls);
        }

        private static string ToBase32(long hs, long ls)
        {
            var charBuffer = new char[26];

            charBuffer[0] = Encode32Chars[(int)(hs >> 60) & 31];
            charBuffer[1] = Encode32Chars[(int)(hs >> 55) & 31];
            charBuffer[2] = Encode32Chars[(int)(hs >> 50) & 31];
            charBuffer[3] = Encode32Chars[(int)(hs >> 45) & 31];
            charBuffer[4] = Encode32Chars[(int)(hs >> 40) & 31];
            charBuffer[5] = Encode32Chars[(int)(hs >> 35) & 31];
            charBuffer[6] = Encode32Chars[(int)(hs >> 30) & 31];
            charBuffer[7] = Encode32Chars[(int)(hs >> 25) & 31];
            charBuffer[8] = Encode32Chars[(int)(hs >> 20) & 31];
            charBuffer[9] = Encode32Chars[(int)(hs >> 15) & 31];
            charBuffer[10] = Encode32Chars[(int)(hs >> 10) & 31];
            charBuffer[11] = Encode32Chars[(int)(hs >> 5) & 31];
            charBuffer[12] = Encode32Chars[(int)hs & 31];

            charBuffer[13] = Encode32Chars[(int)(ls >> 60) & 31];
            charBuffer[14] = Encode32Chars[(int)(ls >> 55) & 31];
            charBuffer[15] = Encode32Chars[(int)(ls >> 50) & 31];
            charBuffer[16] = Encode32Chars[(int)(ls >> 45) & 31];
            charBuffer[17] = Encode32Chars[(int)(ls >> 40) & 31];
            charBuffer[18] = Encode32Chars[(int)(ls >> 35) & 31];
            charBuffer[19] = Encode32Chars[(int)(ls >> 30) & 31];
            charBuffer[20] = Encode32Chars[(int)(ls >> 25) & 31];
            charBuffer[21] = Encode32Chars[(int)(ls >> 20) & 31];
            charBuffer[22] = Encode32Chars[(int)(ls >> 15) & 31];
            charBuffer[23] = Encode32Chars[(int)(ls >> 10) & 31];
            charBuffer[24] = Encode32Chars[(int)(ls >> 5) & 31];
            charBuffer[25] = Encode32Chars[(int)ls & 31];

            return new string(charBuffer);
        }
    }
}
