using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Drivers;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Taxonomies;

public class TaxonomyFieldDisplayDriverRegistrationTests
{
    [Fact]
    public void TaxonomyFieldDisplayDriver_ShouldNotRegisterForCustomDisplayModes()
    {
        var services = new ServiceCollection();
        
        services.AddContentField<TaxonomyField>()
            .UseDisplayDriver<TaxonomyFieldDisplayDriver>(d => string.IsNullOrEmpty(d) || string.Equals(d, "Standard", StringComparison.OrdinalIgnoreCase));
        
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ContentDisplayOptions>>().Value;

        var fieldOptions = options.ContentFieldOptions[nameof(TaxonomyField)];
        Assert.NotNull(fieldOptions);
        
        var defaultDriverResolver = fieldOptions.DisplayModeDrivers.FirstOrDefault(x => x.DisplayDriverType == typeof(TaxonomyFieldDisplayDriver));
        Assert.NotNull(defaultDriverResolver);

        Assert.True(defaultDriverResolver.DisplayMode(null));
        Assert.True(defaultDriverResolver.DisplayMode("Standard"));
        Assert.False(defaultDriverResolver.DisplayMode("Tags"));
        Assert.False(defaultDriverResolver.DisplayMode("Custom"));
    }
}
