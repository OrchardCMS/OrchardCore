using System.Text.Json;

namespace OrchardCore.DisplayManagement.Notify;

public class NotifyJsonSerializerOptions
{
    public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions();
}
