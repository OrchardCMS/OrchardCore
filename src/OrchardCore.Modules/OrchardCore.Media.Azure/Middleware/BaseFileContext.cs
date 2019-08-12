using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using OrchardCore.FileStorage;

namespace OrchardCore.Media.Azure.Middleware
{
    public abstract class BaseFileContext
    {
        protected readonly HttpContext _context;
        protected readonly HttpResponse _response;
        protected readonly string _contentType;
        protected readonly TimeSpan _maxBrowserCacheDays;
        protected readonly string _cacheKey;
        protected readonly string _fileExtension;
        protected readonly PathString _subPath;

        protected readonly RequestHeaders _requestHeaders;
        protected readonly ResponseHeaders _responseHeaders;
        protected readonly bool _isHeadRequest;

        protected EntityTagHeaderValue _etag;

        protected long _length;
        protected DateTimeOffset _lastModified;

        private PreconditionState _ifMatchState;
        private PreconditionState _ifNoneMatchState;
        private PreconditionState _ifModifiedSinceState;
        private PreconditionState _ifUnmodifiedSinceState;

        public BaseFileContext(
            HttpContext context,
            ILogger logger,
            int maxBrowserCacheDays,
            string contentType,
            string cacheKey,
            string fileExtension,
            PathString subPath
            )
        {
            _context = context;
            _response = _context.Response;
            Logger = logger;
            _maxBrowserCacheDays = TimeSpan.FromDays(maxBrowserCacheDays);
            _contentType = contentType;
            _cacheKey = cacheKey;
            _fileExtension = fileExtension;
            _subPath = subPath;
            _requestHeaders = context.Request.GetTypedHeaders();
            _responseHeaders = context.Response.GetTypedHeaders();
            if (HttpMethods.IsHead(context.Request.Method))
            {
                _isHeadRequest = true;
            }
        }

        public ILogger Logger { get; }

        public void ComprehendRequestHeaders()
        {
            ComputeIfMatch();

            ComputeIfModifiedSince();
        }

        private void ComputeIfMatch()
        {
            // 14.24 If-Match
            var ifMatch = _requestHeaders.IfMatch;
            if (ifMatch != null && ifMatch.Any())
            {
                _ifMatchState = PreconditionState.PreconditionFailed;
                foreach (var etag in ifMatch)
                {
                    if (etag.Equals(EntityTagHeaderValue.Any) || etag.Compare(_etag, useStrongComparison: true))
                    {
                        _ifMatchState = PreconditionState.ShouldProcess;
                        break;
                    }
                }
            }

            // 14.26 If-None-Match
            var ifNoneMatch = _requestHeaders.IfNoneMatch;
            if (ifNoneMatch != null && ifNoneMatch.Any())
            {
                _ifNoneMatchState = PreconditionState.ShouldProcess;
                foreach (var etag in ifNoneMatch)
                {
                    if (etag.Equals(EntityTagHeaderValue.Any) || etag.Compare(_etag, useStrongComparison: true))
                    {
                        _ifNoneMatchState = PreconditionState.NotModified;
                        break;
                    }
                }
            }
        }

        private void ComputeIfModifiedSince()
        {
            var now = DateTimeOffset.UtcNow;

            // 14.25 If-Modified-Since
            var ifModifiedSince = _requestHeaders.IfModifiedSince;
            if (ifModifiedSince.HasValue && ifModifiedSince <= now)
            {
                bool modified = ifModifiedSince < _lastModified;
                _ifModifiedSinceState = modified ? PreconditionState.ShouldProcess : PreconditionState.NotModified;
            }

            // 14.28 If-Unmodified-Since
            var ifUnmodifiedSince = _requestHeaders.IfUnmodifiedSince;
            if (ifUnmodifiedSince.HasValue && ifUnmodifiedSince <= now)
            {
                bool unmodified = ifUnmodifiedSince >= _lastModified;
                _ifUnmodifiedSinceState = unmodified ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
            }
        }

        public void ApplyResponseHeaders(int statusCode)
        {
            _response.StatusCode = statusCode;
            if (statusCode < 400)
            {
                // these headers are returned for 200, 206, and 304
                // they are not returned for 412 and 416
                if (!string.IsNullOrEmpty(_contentType))
                {
                    _response.ContentType = _contentType;
                }

                _responseHeaders.LastModified = _lastModified;
                _responseHeaders.ETag = _etag;
                _responseHeaders.Headers[HeaderNames.AcceptRanges] = "bytes";

                // Apply the same cache control headers as ImageSharp.Web.
                _responseHeaders.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = _maxBrowserCacheDays,
                    MustRevalidate = true
                };
            }
            if (statusCode == StatusCodes.Status200OK)
            {
                // this header is only returned here for 200
                // it already set to the returned range for 206
                // it is not returned for 304, 412, and 416
                _response.ContentLength = _length;
            }
        }

        public PreconditionState GetPreconditionState()
            => GetMaxPreconditionState(_ifMatchState, _ifNoneMatchState, _ifModifiedSinceState, _ifUnmodifiedSinceState);

        private static PreconditionState GetMaxPreconditionState(params PreconditionState[] states)
        {
            var max = PreconditionState.Unspecified;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] > max)
                {
                    max = states[i];
                }
            }
            return max;
        }

        public Task SendStatusAsync(int statusCode)
        {
            ApplyResponseHeaders(statusCode);

            Logger.LogDebug("Sending status code {StatusCode} for request path {Path}", statusCode, _subPath);
            return Task.CompletedTask;
        }

        public async Task ServeFile(HttpContext context, RequestDelegate next)
        {
            ComprehendRequestHeaders();
            switch (GetPreconditionState())
            {
                case PreconditionState.Unspecified:
                case PreconditionState.ShouldProcess:
                    if (_isHeadRequest)
                    {
                        await SendStatusAsync(StatusCodes.Status200OK);
                        return;
                    }

                    try
                    {
                        await SendAsync();
                        return;
                    }
                    catch (FileNotFoundException)
                    {
                        context.Response.Clear();
                    }

                    catch (FileStoreException)
                    {
                        context.Response.Clear();
                    }
                    await next(context);
                    return;
                case PreconditionState.NotModified:
                    Logger.LogDebug("File not modified for request path {Path}", _subPath);
                    await SendStatusAsync(StatusCodes.Status304NotModified);
                    return;
                case PreconditionState.PreconditionFailed:
                    Logger.LogDebug("Precondition failed for request path {Path}", _subPath);
                    await SendStatusAsync(StatusCodes.Status412PreconditionFailed);
                    return;
                default:
                    var exception = new NotImplementedException(GetPreconditionState().ToString());
                    Debug.Fail(exception.ToString());
                    throw exception;
            }
        }

        public abstract Task SendAsync();

        public enum PreconditionState : byte
        {
            Unspecified,
            NotModified,
            ShouldProcess,
            PreconditionFailed
        }
    }
}
