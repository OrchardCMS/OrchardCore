using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Tests.Mvc.ModelBinding;

public class CommaSeparatedStringArrayModelBinderTests
{
    [Fact]
    public async Task BindModelAsyncCombinesRepeatedAndCommaSeparatedValues()
    {
        var bindingContext = CreateBindingContext(new StringValues([" Article, ,BlogPost ", "Page"]));
        var binder = new CommaSeparatedStringArrayModelBinder();

        await binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        Assert.Equal(["Article", "BlogPost", "Page"], Assert.IsType<string[]>(bindingContext.Result.Model));
    }

    [Fact]
    public async Task BindModelAsyncReturnsNullWhenValueIsMissing()
    {
        var bindingContext = CreateBindingContext();
        var binder = new CommaSeparatedStringArrayModelBinder();

        await binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        Assert.Null(bindingContext.Result.Model);
    }

    [Fact]
    public async Task BindModelAsyncPreservesSingleValueCompatibility()
    {
        var bindingContext = CreateBindingContext("Article");
        var binder = new CommaSeparatedStringArrayModelBinder();

        await binder.BindModelAsync(bindingContext);

        Assert.Equal(["Article"], Assert.IsType<string[]>(bindingContext.Result.Model));
    }

    [Fact]
    public async Task BindModelAsyncReturnsNullWhenValuesAreEmpty()
    {
        var bindingContext = CreateBindingContext(" , ");
        var binder = new CommaSeparatedStringArrayModelBinder();

        await binder.BindModelAsync(bindingContext);

        Assert.Null(bindingContext.Result.Model);
    }

    private static DefaultModelBindingContext CreateBindingContext(StringValues values = default)
    {
        var valueProvider = new QueryStringValueProvider(
            BindingSource.Query,
            new QueryCollection(values.Count > 0
                ? new Dictionary<string, StringValues> { ["value"] = values }
                : []),
            CultureInfo.InvariantCulture);

        return new DefaultModelBindingContext
        {
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string[])),
            ModelName = "value",
            ModelState = new ModelStateDictionary(),
            ValueProvider = valueProvider,
        };
    }
}
