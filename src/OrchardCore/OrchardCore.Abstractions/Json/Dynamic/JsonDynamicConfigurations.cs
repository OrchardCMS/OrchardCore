using System.Collections.Generic;

namespace OrchardCore.Json.Dynamic;
public static class JsonDynamicConfigurations
{
    public static HashSet<IJsonDynamicValueHandler> ValueHandlers { get; } = [new DefaultJsonDyanmicValueHandler()];
}
