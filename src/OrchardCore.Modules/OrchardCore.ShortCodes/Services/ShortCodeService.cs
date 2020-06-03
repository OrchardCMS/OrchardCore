using System.Collections.Generic;
using System.IO;
using System.Text;
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
            var stringBuilder = new StringBuilder(input);
            var shortCodeContext = new ShortCodeContext();
            var shortCodeParser = new ShortCodeParser();
            var shortCodeOccurences = shortCodeParser.Parse(stringBuilder.ToString());
            foreach (var shortCodeOccurence in shortCodeOccurences)
            {
                var shortCodeOutput = new ShortCodeOutput(shortCode, encoder =>
                {
                    var shortCodeContent = new DefaultShortCodeContent();
                    shortCodeContent.SetHtmlContent(shortCodeOccurence.Text);

                    return Task.FromResult<ShortCodeContent>(shortCodeContent);
                });

                await shortCode.ProcessAsync(shortCodeContext, shortCodeOutput);

                using (var writer = new StringWriter())
                {
                    shortCodeOutput.WriteTo(writer, HtmlEncoder.Default);
                    stringBuilder.Replace(shortCodeOccurence.Text, writer.ToString());
                }
            }

            return stringBuilder.ToString();
        }
    }
}
