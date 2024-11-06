using Cysharp.Text;
using UglyToad.PdfPig;

namespace OrchardCore.Media.Indexing;

public class PdfMediaFileTextProvider : IMediaFileTextProvider
{
    public async Task<string> GetTextAsync(string path, Stream fileStream)
    {
        // PdfPig requires the stream to be seekable, see:
        // https://github.com/UglyToad/PdfPig/blob/master/src/UglyToad.PdfPig.Core/StreamInputBytes.cs#L45.
        // Thus if it isn't, which is the case with e.g. Azure Blob Storage, we need to copy it to a new, seekable
        // Stream.
        FileStream seekableStream = null;
        try
        {
            if (!fileStream.CanSeek)
            {
                seekableStream = GetTemporaryFileStream();

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

                await File.DeleteAsync(seekableStream.Name);
            }
        }
    }

    private static FileStream GetTemporaryFileStream()
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

        return new FileStream(tempFilePath, FileMode.Create, FileAccess.Write);
    }
}
