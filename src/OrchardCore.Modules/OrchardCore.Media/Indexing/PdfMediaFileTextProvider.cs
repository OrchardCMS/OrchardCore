using System.IO;
using System.Threading.Tasks;
using Cysharp.Text;
using UglyToad.PdfPig;

namespace OrchardCore.Media.Indexing
{
    public class PdfMediaFileTextProvider : IMediaFileTextProvider
    {
        public async Task<string> GetTextAsync(string path, Stream fileStream)
        {
            // PdfPig requires the stream to be seekable, see:
            // https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.Core/StreamInputBytes.cs#L45.
            // Thus if it isn't, which is the case with e.g. Azure Blob Storage, we need to copy it to a new, seekable
            // Stream.
            MemoryStream seekableStream = null;
            try
            {
                if (!fileStream.CanSeek)
                {
                    // Since fileStream.Length might not be supported either, we can't preconfigure the capacity of the
                    // MemoryStream.
                    seekableStream = new MemoryStream();
                    // While this involves loading the file into memory, we don't really have a choice.
                    await fileStream.CopyToAsync(seekableStream);
                    seekableStream.Position = 0;
                }

                using var document = PdfDocument.Open(seekableStream ?? fileStream);
                using var stringBuilder = ZString.CreateStringBuilder();

                foreach (var page in document.GetPages())
                {
                    stringBuilder.Append(page.Text);
                }

                return stringBuilder.ToString();
            }
            finally
            {
                if (seekableStream != null)
                {
                    await seekableStream.DisposeAsync();
                }
            }
        }
    }
}
