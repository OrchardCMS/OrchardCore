namespace OrchardCore.Entities;

public interface ICacheableEntity : IEntity
{
    void Set(string key, object value);

    void Remove(string key);

    object Get(string key);
}
