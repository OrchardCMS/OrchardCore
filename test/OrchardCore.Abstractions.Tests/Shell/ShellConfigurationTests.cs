using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration.Internal;
using Xunit;

#nullable enable

namespace OrchardCore.Environment.Shell.Configuration;

public class ShellConfigurationTests
{
    [Fact]
    public void Indexer_MissingKey_ReturnsNull()
    {
        var configuration = new ShellConfiguration();

        try
        {
            Assert.Null(configuration["Missing"]);
        }
        finally
        {
            configuration.Release();
        }
    }

    [Fact]
    public void Indexer_NullValue_PreservesKey()
    {
        var configuration = new ShellConfiguration();

        try
        {
            configuration["Setting"] = "value";
            configuration["Setting"] = null;

            Assert.Null(configuration["Setting"]);
            Assert.Equal("Setting", Assert.Single(configuration.GetChildren()).Key);
        }
        finally
        {
            configuration.Release();
        }
    }

    [Fact]
    public void Copy_InitializedConfiguration_PreservesNullValues()
    {
        var configuration = new ShellConfiguration();
        ShellConfiguration? copy = null;

        try
        {
            configuration["Setting"] = null;
            copy = new ShellConfiguration(configuration);

            Assert.Null(copy["Setting"]);
            Assert.Equal("Setting", Assert.Single(copy.GetChildren()).Key);
        }
        finally
        {
            copy?.Release();
            configuration.Release();
        }
    }

    [Fact]
    public async Task Copy_LazyConfiguration_UsesNewTenantName()
    {
        var configuration = new ShellConfiguration("Original", (name, configure) =>
        {
            var builder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tenant"] = name,
            });
            configure(builder);
            return Task.FromResult(builder.Build());
        });
        var copy = new ShellConfiguration("Copy", configuration);

        try
        {
            await copy.EnsureConfigurationAsync();

            Assert.Equal("Copy", copy["Tenant"]);
            Assert.Equal("Original", configuration["Tenant"]);
        }
        finally
        {
            copy.Release();
            configuration.Release();
        }
    }

    [Fact]
    public void ConfigurationData_NullDictionaryValues_PreservesJsonNull()
    {
        var data = new Dictionary<string, string?>
        {
            ["Missing"] = null,
            ["Setting"] = "value",
        };

        var result = data.ToJsonObject();

        Assert.True(result.ContainsKey("Missing"));
        Assert.Null(result["Missing"]);
        Assert.Equal("value", result["Setting"]!.GetValue<string>());
    }

    [Fact]
    public async Task ToConfigurationDataAsync_NullConfiguration_ReturnsEmptyDictionary()
    {
        JsonObject? configuration = null;

        var data = await configuration.ToConfigurationDataAsync();

        Assert.Empty(data);
    }
}
