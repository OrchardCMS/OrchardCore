using System;
using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class TablePrefixGenerator
	{
        public async Task<string> GeneratePrefixAsync()
        {
            // or use this Path.GetRandomFileName(). depends how long it is.
            // TODO a short delay possibly?
            await Task.Delay(1);
            long ticks = DateTime.Now.Ticks;
            byte[] bytes = BitConverter.GetBytes(ticks);
            // TODO can remove the O if we can guarantee it won't start with a number.
            // Tables can't start with a number.
            // But at this point we are (so far) under the Yes.Sql limits of table name / foreign key name
            // Need O sometimes starts with a number.
            string id = 'O' + Convert.ToBase64String(bytes)
                                .Replace('+', '_')
                                // .Replace('/', '-') // - not allowed in mysql. need alternative.
                                .Replace("/", "__") // - not allowed in mysql. need alternative.
                                .TrimEnd('=');
            return id;
        }
    }
}
