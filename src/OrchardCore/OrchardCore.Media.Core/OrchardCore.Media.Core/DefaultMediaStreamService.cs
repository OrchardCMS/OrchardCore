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
            ILogger<DefaultMediaFileStore> logger,
            IEnumerable<IMediaEventHandler> mediaEventHandlers,
            IMediaFileStore mediaFileStore
            )
        {          
            Logger = logger;
            _mediaEventHandlers = mediaEventHandlers;
            _mediaFileStore = mediaFileStore;
        }

        public ILogger Logger { get; }
        public async Task Preprocess(MediaCreatingContext mediaCreatingContext)
        {                       
            
            if(mediaCreatingContext.NeedPreprocess)
            {
                _mediaEventHandlers.Invoke((handler, context) => handler.MediaCreating(context), mediaCreatingContext, Logger);
            }

            await _mediaFileStore.CreateFileFromStreamAsync(mediaCreatingContext.Path, mediaCreatingContext.Stream);
            
        }


    }
}


