namespace OrchardCore.ContentManagement;

public class ContentPartOption : ContentPartOptionBase
{
    private readonly List<Type> _handlers = [];

    public ContentPartOption(Type contentPartType) : base(contentPartType)
    {
    }

    public IReadOnlyList<Type> Handlers => _handlers;

    internal void AddHandler(Type type)
    {
        _handlers.Add(type);
    }

    internal void RemoveHandler(Type type)
    {
        _handlers.Remove(type);
    }
}
