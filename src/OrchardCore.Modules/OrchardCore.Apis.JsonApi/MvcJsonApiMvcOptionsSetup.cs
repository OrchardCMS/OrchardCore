using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace OrchardCore.Apis.JsonApi
{
    public class MvcJsonApiMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ArrayPool<char> _charPool;
        private readonly ObjectPoolProvider _objectPoolProvider;

        public MvcJsonApiMvcOptionsSetup(
            ILoggerFactory loggerFactory,
            IOptions<MvcJsonOptions> jsonOptions,
            ArrayPool<char> charPool,
            ObjectPoolProvider objectPoolProvider)
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
            _jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
            _charPool = charPool;
            _objectPoolProvider = objectPoolProvider;
        }

        public void Configure(MvcOptions options)
        {
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter(_jsonSerializerSettings, _charPool));

            var jsonInputLogger = _loggerFactory.CreateLogger<JsonApiInputFormatter>();
            options.InputFormatters.Insert(0, new JsonApiInputFormatter(
                jsonInputLogger,
                _jsonSerializerSettings,
                _charPool,
                _objectPoolProvider
                ));

            options.FormatterMappings.SetMediaTypeMappingForFormat("jsonapi", MediaTypeHeaderValues.ApplicationJsonApi);
        }
    }
}
