using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Media.Events;
using OrchardCore.Modules;

namespace OrchardCore.Media.Core
{
    public class DefaultMediaStreamService : IMediaStreamService
    {      
        private readonly IEnumerable<IMediaEventHandler> _mediaEventHandlers;
        private readonly IMediaFileStore _mediaFileStore;
        public DefaultMediaStreamService(           
            ILogger<DefaultMediaStreamService> logger,
            IEnumerable<IMediaEventHandler> mediaEventHandlers,
            IMediaFileStore mediaFileStore
            )
        {          
            Logger = logger;
            _mediaEventHandlers = mediaEventHandlers;
            _mediaFileStore = mediaFileStore;
        }

        public ILogger Logger { get; }
        public async Task<OutputStream> CreateFileFromStreamAsync(MediaCreationContext mediaContext)
        {                       
            
            if(mediaContext.NeedToBePreprocessed)
            {
                _mediaEventHandlers.Invoke((handler, context) => handler.MediaCreating(context), mediaContext, Logger);
            }

            await _mediaFileStore.CreateFileFromStreamAsync(mediaContext.Path, mediaContext.Stream);

            OutputStream outputImage = new OutputStream();
            outputImage.Stream = mediaContext.Stream;
            outputImage.Width = mediaContext.OutputWidth;
            outputImage.Height = mediaContext.OutputHeight;            

            if (mediaContext.NeedToBePostprocessed)
            {
                _mediaEventHandlers.Invoke((handler, context) => handler.MediaCreated(context), mediaContext, Logger);
            }

            return outputImage;

        }

        public async Task<bool> TryDeleteFileAsync(MediaContext mediaContext)
        {
            if (mediaContext.NeedToBePreprocessed)
            {
                _mediaEventHandlers.Invoke((handler, context) => handler.MediaDeleting(context), mediaContext, Logger);
            }

            bool result = await _mediaFileStore.TryDeleteFileAsync(mediaContext.Path);

            if (mediaContext.NeedToBePostprocessed)
            {
                if (result)
                {
                    _mediaEventHandlers.Invoke((handler, context) => handler.MediaDeleted(context), mediaContext, Logger);
                }
                else
                {
                    _mediaEventHandlers.Invoke((handler, context) => handler.MediaDeletedUncomplete(context), mediaContext, Logger);
                }

            }      

            return result;

        }


    }
}


