using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Media.Events
{
    /// <summary>
    /// Event handler fired during creation of new media items.
    /// </summary>
    public interface IMediaCreatingEventHandler
    {
        /// <summary>
        /// Allows a stream to be mutated during creation of media.
        /// Any implementation must return a new stream,
        /// which should be disposed by the caller.
        /// </summary>
        Task<Stream> MediaCreatingAsync(MediaCreatingContext context, Stream creatingStream);
    }
}
