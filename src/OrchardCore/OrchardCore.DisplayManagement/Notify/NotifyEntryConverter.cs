using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Notify;

public class NotifyEntryConverter : JsonConverter<NotifyEntry>
{
    private readonly HtmlEncoder _htmlEncoder;

    public NotifyEntryConverter(HtmlEncoder htmlEncoder)
    {
        _htmlEncoder = htmlEncoder;
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(NotifyEntry).IsAssignableFrom(objectType);
    }

    public override NotifyEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jo = JObject.Load(ref reader);

        var notifyEntry = new NotifyEntry
        {
            Message = new HtmlString(jo.Value<string>("Message")),
        };

        if (Enum.TryParse<NotifyType>(jo.Value<string>("Type"), out var type))
        {
            notifyEntry.Type = type;
        }

        return notifyEntry;
    }

    public override void Write(Utf8JsonWriter writer, NotifyEntry value, JsonSerializerOptions options)
    {
        var notifyEntry = value;
        if (notifyEntry == null)
        {
            return;
        }

        var o = new JsonObject
        {
            { nameof(NotifyEntry.Type), notifyEntry.Type.ToString() },
            { nameof(NotifyEntry.Message), notifyEntry.ToHtmlString(_htmlEncoder) }
        };

        o.WriteTo(writer);
    }
}
