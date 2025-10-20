namespace OrchardCore.Catalogs;

public interface ICloneable<T> : ICloneable
{
    new T Clone();

    object ICloneable.Clone()
        => Clone();
}
