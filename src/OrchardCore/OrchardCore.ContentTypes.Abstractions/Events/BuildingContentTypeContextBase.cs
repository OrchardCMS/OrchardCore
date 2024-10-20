namespace OrchardCore.ContentTypes.Events;

public abstract class BuildingContentTypeContextBase
{
    public readonly string Name;

    protected BuildingContentTypeContextBase(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }
}
