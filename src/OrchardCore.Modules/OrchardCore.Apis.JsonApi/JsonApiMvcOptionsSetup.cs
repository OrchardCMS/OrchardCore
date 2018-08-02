using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace OrchardCore.Apis.JsonApi
{
    public class JsonApiMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly MvcJsonOptions _jsonOptions;
        private readonly ArrayPool<char> _charPool;
        private readonly ObjectPoolProvider _objectPoolProvider;
        private readonly IUrlHelperFactory _factory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public JsonApiMvcOptionsSetup(
            ILoggerFactory loggerFactory,
            IOptions<MvcJsonOptions> jsonOptions,
            ArrayPool<char> charPool,
            ObjectPoolProvider objectPoolProvider,
            IUrlHelperFactory factory,
            IActionContextAccessor actionContextAccessor)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (jsonOptions == null)
            {
                throw new ArgumentNullException(nameof(jsonOptions));
            }

            if (charPool == null)
            {
                throw new ArgumentNullException(nameof(charPool));
            }

            if (objectPoolProvider == null)
            {
                throw new ArgumentNullException(nameof(objectPoolProvider));
            }

            _loggerFactory = loggerFactory;
            _jsonOptions = jsonOptions.Value;
            _charPool = charPool;
            _objectPoolProvider = objectPoolProvider;
            _factory = factory;
            _actionContextAccessor = actionContextAccessor;
        }

        public void Configure(MvcOptions options)
        {
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter(
                _factory,
                _actionContextAccessor,
                _jsonOptions.SerializerSettings, 
                _charPool));

            var jsonInputLogger = _loggerFactory.CreateLogger<JsonApiInputFormatter>();
            options.InputFormatters.Insert(0, new JsonApiInputFormatter(
                jsonInputLogger,
                _jsonOptions.SerializerSettings,
                _charPool,
                _objectPoolProvider,
                options,
                _jsonOptions
                ));

            options.FormatterMappings.SetMediaTypeMappingForFormat("jsonapi", MediaTypeHeaderValues.ApplicationJsonApi);
        }
    }
}
