using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding;

internal sealed class SafeBoolModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(bool))
        {
            return new SafeBoolModelBinder();
        }

        return null;
    }
}
