using System.Text.Json.Nodes;

namespace OrchardCore.Entities;

public interface IEntity
{
    JsonObject Properties { get; }
}
