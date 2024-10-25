using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Notifications.Services;

public class NotificationFilterEngineModelBinder : IModelBinder
{
    private readonly INotificationAdminListFilterParser _parser;

    public NotificationFilterEngineModelBinder(INotificationAdminListFilterParser parser)
    {
        _parser = parser;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;

        // Try to fetch the value of the argument by name q=
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(_parser.Parse(string.Empty));

            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        // When value is null or empty the parser returns an empty result.
        bindingContext.Result = ModelBindingResult.Success(_parser.Parse(valueProviderResult.FirstValue));

        return Task.CompletedTask;
    }
}
