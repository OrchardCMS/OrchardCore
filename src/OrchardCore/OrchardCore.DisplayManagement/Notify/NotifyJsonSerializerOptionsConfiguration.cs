using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace OrchardCore.DisplayManagement.Notify;

internal sealed class NotifyJsonSerializerOptionsConfiguration : IConfigureOptions<NotifyJsonSerializerOptions>
{
    private readonly HtmlEncoder _htmlEncoder;

    public NotifyJsonSerializerOptionsConfiguration(HtmlEncoder htmlEncoder)
    {
        _htmlEncoder = htmlEncoder;
    }

    public void Configure(NotifyJsonSerializerOptions options)
    {
        options.SerializerOptions.Converters.Add(new NotifyEntryConverter(_htmlEncoder));
    }
}
