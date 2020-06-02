using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OrchardCore.ShortCodes.Services
{
    public class ShortCodeService : IShortCodeService
    {
        private readonly IEnumerable<IShortCode> _shortCodes;

        public ShortCodeService(IEnumerable<IShortCode> shortCodes)
        {
            _shortCodes = shortCodes;
        }

        public async ValueTask<string> ProcessAsync(string input)
        {
            foreach (var shortCode in _shortCodes)
            {
                input = await GetContentAsync(shortCode, input);
            }

            return input;
        }

        private async Task<string> GetContentAsync(IShortCode shortCode, string input)
        {
            var shortCodeContext = new ShortCodeContext(shortCode.Name);
            var shortCodeOutput = new ShortCodeOutput(shortCode.Name, encoder =>
            {
                var shortCodeContent = new DefaultShortCodeContent();
                shortCodeContent.SetHtmlContent(input);

                return Task.FromResult<ShortCodeContent>(shortCodeContent);
            });

            await shortCode.ProcessAsync(shortCodeContext, shortCodeOutput);

            using (var writer = new StringWriter())
            {
                shortCodeOutput.WriteTo(writer, HtmlEncoder.Default);

                return writer.ToString();
            }
        }
    }
}
