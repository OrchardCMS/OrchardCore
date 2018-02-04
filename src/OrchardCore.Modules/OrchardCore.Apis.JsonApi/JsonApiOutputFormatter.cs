using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace OrchardCore.Apis.JsonApi
{
    /// <summary>
    /// A <see cref="TextOutputFormatter"/> for JSON content.
    /// </summary>
    public class JsonApiOutputFormatter : TextOutputFormatter
    {
        private readonly IArrayPool<char> _charPool;

        // Perf: JsonSerializers are relatively expensive to create, and are thread safe. We cache
        // the serializer and invalidate it when the settings change.
        private JsonSerializer _serializer;

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        /// <summary>
        /// Initializes a new <see cref="JsonOutputFormatter"/> instance.
        /// </summary>
        /// <param name="serializerSettings">
        /// The <see cref="JsonSerializerSettings"/>. Should be either the application-wide settings
        /// (<see cref="MvcJsonOptions.SerializerSettings"/>) or an instance
        /// <see cref="JsonSerializerSettingsProvider.CreateSerializerSettings"/> initially returned.
        /// </param>
        /// <param name="charPool">The <see cref="ArrayPool{Char}"/>.</param>
        public JsonApiOutputFormatter(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            JsonSerializerSettings serializerSettings, 
            ArrayPool<char> charPool)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            if (charPool == null)
            {
                throw new ArgumentNullException(nameof(charPool));
            }

            SerializerSettings = serializerSettings;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _charPool = new JsonArrayPool<char>(charPool);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJsonApi);
        }

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// Any modifications to the <see cref="JsonSerializerSettings"/> object after this
        /// <see cref="JsonOutputFormatter"/> has been used will have no effect.
        /// </remarks>
        protected JsonSerializerSettings SerializerSettings { get; }

        /// <summary>
        /// Writes the given <paramref name="value"/> as JSON using the given
        /// <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to write the <paramref name="value"/></param>
        /// <param name="value">The value to write as JSON.</param>
        public async Task WriteObject(TextWriter writer, IJsonApiResultManager manager, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var document = await manager.Build(urlHelper, value);

            using (var jsonWriter = CreateJsonWriter(writer))
            {
                var jsonSerializer = CreateJsonSerializer();
                jsonSerializer.Serialize(jsonWriter, document);
            }
        }

        /// <summary>
        /// Called during serialization to create the <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to write.</param>
        /// <returns>The <see cref="JsonWriter"/> used during serialization.</returns>
        protected virtual JsonWriter CreateJsonWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var jsonWriter = new JsonTextWriter(writer)
            {
                ArrayPool = _charPool,
                CloseOutput = false,
                AutoCompleteOnClose = false
            };

            return jsonWriter;
        }

        /// <summary>
        /// Called during serialization to create the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <returns>The <see cref="JsonSerializer"/> used during serialization and deserialization.</returns>
        protected virtual JsonSerializer CreateJsonSerializer()
        {
            if (_serializer == null)
            {
                _serializer = JsonSerializer.Create(SerializerSettings);
            }

            return _serializer;
        }

        /// <inheritdoc />
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            var response = context.HttpContext.Response;
            var manager = context.HttpContext.RequestServices.GetService<IJsonApiResultManager>();
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                await WriteObject(writer, manager, context.Object);

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous
                // write).
                await writer.FlushAsync();
            }
        }
    }
}
