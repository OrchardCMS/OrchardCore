using OrchardCore.Media.Events;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class MediaEventTests
    {
        [Fact]
        public async Task DisposesMediaCreatingStreams()
        {
            var streams = new List<Stream>();
            var creatingEventHandlers = new List<IMediaCreatingEventHandler>()
            {
                new TestMediaEventHandler(),
                new TestMediaEventHandler()
            };

            Stream originalStream = null;

            var path = String.Empty;

            // This stream will be disposed by the creating stream, or the finally block.
            Stream inputStream = null;
            try
            {
                inputStream = new MemoryStream();
                originalStream = inputStream;

                // Add original stream to streams to maintain reference to test disposal.
                streams.Add(originalStream);

                var outputStream = inputStream;
                try
                {
                    var context = new MediaCreatingContext
                    {
                        Path = path
                    };

                    foreach (var eventHandler in creatingEventHandlers)
                    {
                        // Creating stream disposed by using.
                        using var creatingStream = outputStream;

                        // Add to streams to maintain reference to test disposal.
                        streams.Add(creatingStream);
                        inputStream = null;
                        outputStream = null;
                        outputStream = await eventHandler.MediaCreatingAsync(context, creatingStream);
                    }
                }
                finally
                {
                    // This disposes the final outputStream.
                    if (outputStream != null)
                    {
                        await outputStream.DisposeAsync();
                    }
                }
            }
            finally
            {
                if (inputStream != null)
                {
                    await inputStream.DisposeAsync();
                }
            }

            foreach (var stream in streams)
            {
                Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
            }
        }
    }

    public class TestMediaEventHandler : IMediaCreatingEventHandler
    {
        public async Task<Stream> MediaCreatingAsync(MediaCreatingContext context, Stream inputStream)
        {
            var outStream = new MemoryStream();
            await inputStream.CopyToAsync(outStream);

            return outStream;
        }
    }
}
