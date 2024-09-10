using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.Services;

public class UserFilterEngineModelBinder : IModelBinder
{
    private readonly IUsersAdminListFilterParser _parser;

    public UserFilterEngineModelBinder(IUsersAdminListFilterParser parser)
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

        var value = valueProviderResult.FirstValue;

        // When value is null or empty the parser returns an empty result.
        bindingContext.Result = ModelBindingResult.Success(_parser.Parse(value));

        return Task.CompletedTask;
    }
}
