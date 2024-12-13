namespace OrchardCore.ContentManagement;

internal sealed class GenericTypeActivator<T, TInstance> : ITypeActivator<TInstance> where T : TInstance, new()
{
    /// <inheritdoc />
    public Type Type => typeof(T);

    /// <inheritdoc />
    public TInstance CreateInstance()
    {
        return new T();
    }
}
