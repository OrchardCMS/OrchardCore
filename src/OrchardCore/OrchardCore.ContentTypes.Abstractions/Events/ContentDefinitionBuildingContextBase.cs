namespace OrchardCore.ContentTypes.Events;

public abstract class ContentDefinitionBuildingContextBase
{
    public readonly string Name;

    protected ContentDefinitionBuildingContextBase(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }
}
