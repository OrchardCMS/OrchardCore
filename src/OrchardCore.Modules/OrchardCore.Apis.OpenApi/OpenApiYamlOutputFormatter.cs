using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;

namespace OrchardCore.Apis.OpenApi
{
    /// <summary>
    /// A <see cref="TextOutputFormatter"/> for JSON content.
    /// </summary>
    public class OpenApiYamlOutputFormatter : TextOutputFormatter
    {

        /// <summary>
        /// Initializes a new <see cref="OpenApiYamlOutputFormatter"/> instance.
        /// </summary>
        public OpenApiYamlOutputFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationYaml);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextYaml);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(OpenApiDocument).IsAssignableFrom(type);
        }

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// Any modifications to the <see cref="JsonSerializerSettings"/> object after this
        /// <see cref="JsonOutputFormatter"/> has been used will have no effect.
        /// </remarks>
        protected JsonSerializerSettings SerializerSettings { get; }

        public void WriteObject(TextWriter writer, OpenApiDocument value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            value.SerializeAsV3(CreateWriter(writer));
        }

        /// <summary>
        /// Called during serialization to create the <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to write.</param>
        /// <returns>The <see cref="JsonWriter"/> used during serialization.</returns>
        protected virtual IOpenApiWriter CreateWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            return new OpenApiYamlWriter(writer);
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
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                WriteObject(writer, (OpenApiDocument)context.Object);

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous
                // write).
                await writer.FlushAsync();
            }
        }
    }
}
