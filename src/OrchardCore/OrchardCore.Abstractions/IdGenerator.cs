using System;

namespace OrchardCore
{
    public static class IdGenerator
    {
        // Some confusing chars are ignored: http://www.crockford.com/wrmg/base32.html
        private static readonly char[] _encode32Chars = "0123456789abcdefghjkmnpqrstvwxyz".ToCharArray();

        public static string GenerateId()
        {
            var guid = Guid.NewGuid().ToByteArray();

            return String.Create(26, guid, (buffer, guid) =>
            {
                var hs = BitConverter.ToInt64(guid, 0);
                var ls = BitConverter.ToInt64(guid, 8);

                buffer[0] = _encode32Chars[(hs >> 60) & 31];
                buffer[1] = _encode32Chars[(hs >> 55) & 31];
                buffer[2] = _encode32Chars[(hs >> 50) & 31];
                buffer[3] = _encode32Chars[(hs >> 45) & 31];
                buffer[4] = _encode32Chars[(hs >> 40) & 31];
                buffer[5] = _encode32Chars[(hs >> 35) & 31];
                buffer[6] = _encode32Chars[(hs >> 30) & 31];
                buffer[7] = _encode32Chars[(hs >> 25) & 31];
                buffer[8] = _encode32Chars[(hs >> 20) & 31];
                buffer[9] = _encode32Chars[(hs >> 15) & 31];
                buffer[10] = _encode32Chars[(hs >> 10) & 31];
                buffer[11] = _encode32Chars[(hs >> 5) & 31];
                buffer[12] = _encode32Chars[hs & 31];

                buffer[13] = _encode32Chars[(ls >> 60) & 31];
                buffer[14] = _encode32Chars[(ls >> 55) & 31];
                buffer[15] = _encode32Chars[(ls >> 50) & 31];
                buffer[16] = _encode32Chars[(ls >> 45) & 31];
                buffer[17] = _encode32Chars[(ls >> 40) & 31];
                buffer[18] = _encode32Chars[(ls >> 35) & 31];
                buffer[19] = _encode32Chars[(ls >> 30) & 31];
                buffer[20] = _encode32Chars[(ls >> 25) & 31];
                buffer[21] = _encode32Chars[(ls >> 20) & 31];
                buffer[22] = _encode32Chars[(ls >> 15) & 31];
                buffer[23] = _encode32Chars[(ls >> 10) & 31];
                buffer[24] = _encode32Chars[(ls >> 5) & 31];
                buffer[25] = _encode32Chars[ls & 31];
            });
        }
    }
}
