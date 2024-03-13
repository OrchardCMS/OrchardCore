using System.Text.Json.Nodes;

namespace OrchardCore.Entities;

public class Entity : IEntity
{
    public JsonObject Properties { get; set; } = [];
}
