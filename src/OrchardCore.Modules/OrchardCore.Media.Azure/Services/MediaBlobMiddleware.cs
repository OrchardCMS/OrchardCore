using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Media.Azure.Services
{
    public class MediaBlobMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMediaFileStore _mediaFileStore;

        public MediaBlobMiddleware(
            RequestDelegate next,
            IMediaFileStore mediaFileStore
            )
        {
            _next = next;
            _mediaFileStore = mediaFileStore;
            
        }
        public async Task Invoke(HttpContext context)
        {
            // Just experimenting
            // This could use MediaOptions and supported extensions to validation
            var mappedPath = _mediaFileStore.MapPublicUrlToPath(context.Request.PathBase + context.Request.Path);
            var fileInfo = await _mediaFileStore.GetFileInfoAsync(mappedPath);
            if (fileInfo == null)
            {
                await _next.Invoke(context);
                return;
            }

            var fileStream = await _mediaFileStore.GetFileStreamAsync(fileInfo);
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            await fileStream.CopyToAsync(context.Response.Body);
            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Position = 0;
            }
            context.Response.StatusCode = 200;

        }
    }
}

