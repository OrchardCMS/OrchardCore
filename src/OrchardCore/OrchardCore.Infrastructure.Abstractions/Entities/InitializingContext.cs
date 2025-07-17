using System.Text.Json.Nodes;

namespace OrchardCore.Infrastructure.Entities;

public sealed class InitializingContext<T> : HandlerContextBase<T>
{
    public JsonNode Data { get; }

    public InitializingContext(T model, JsonNode data)
        : base(model)
    {
        Data = data ?? new JsonObject();
    }
}
