using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding;

/// <summary>
/// An <see cref="IModelBinderProvider"/> for <see cref="CheckMarkModelBinder"/>.
/// </summary>
public class CheckMarkModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(CheckMarkModelBinder))
        {
            return new CheckMarkModelBinder();
        }

        return null;
    }
}
