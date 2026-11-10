using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Tests.Shell.Configuration;

public class ConfigurationSectionExtensionsTests
{
    [Fact]
    public void GetSectionCompat_OnlyLegacySection_ReturnsLegacyValues()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Media_Azure:ContainerName"] = "legacy",
        });

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.True(section.Exists());
        Assert.Equal("legacy", section["ContainerName"]);
    }

    [Fact]
    public void GetSectionCompat_OnlySection_ReturnsSectionValues()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["Media:Azure:ContainerName"] = "current",
        });

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.True(section.Exists());
        Assert.Equal("current", section["ContainerName"]);
    }

    [Fact]
    public void GetSectionCompat_BothSections_SectionValuesWin()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Media_Azure:ConnectionString"] = "legacy-connection",
            ["OrchardCore_Media_Azure:ContainerName"] = "legacy",
            ["Media:Azure:ContainerName"] = "current",
        });

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.Equal("current", section["ContainerName"]);
        Assert.Equal("legacy-connection", section["ConnectionString"]);
        Assert.Equal("current", section.GetValue<string>("ContainerName"));
    }

    [Fact]
    public void GetSectionCompat_EmptySectionValue_WinsOverLegacyValue()
    {
        // A shell configuration ignores empty values of the application configuration, so a plain configuration is used.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore_Media_Azure:ContainerName"] = "legacy",
                ["Media:Azure:ContainerName"] = string.Empty,
            })
            .Build();

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.Equal(string.Empty, section["ContainerName"]);
    }

    [Fact]
    public void GetSectionCompat_LegacyDotNotation_ReturnsLegacyValues()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore.Media.Azure:ContainerName"] = "dotted",
        });

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.Equal("dotted", section["ContainerName"]);
    }

    [Fact]
    public void GetSectionCompat_NoSection_DoesNotExist()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>());

        var section = configuration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure");

        Assert.False(section.Exists());
        Assert.Null(section.Get<TestOptions>());
        Assert.Equal("Media:Azure", section.Path);
        Assert.Equal("Azure", section.Key);
    }

    [Fact]
    public void GetSectionCompat_GetChildren_MergesChildrenOfBothSections()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Media_Azure:ContainerName"] = "legacy",
            ["OrchardCore_Media_Azure:BasePath"] = "legacy",
            ["Media:Azure:containername"] = "current",
            ["Media:Azure:CreateContainer"] = "true",
        });

        var children = configuration
            .GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure")
            .GetChildren()
            .ToDictionary(child => child.Key, child => child.Value, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(3, children.Count);
        Assert.Equal("current", children["ContainerName"]);
        Assert.Equal("legacy", children["BasePath"]);
        Assert.Equal("true", children["CreateContainer"]);
    }

    [Fact]
    public void GetSectionCompat_Bind_BindsValuesOfBothSections()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Test:Name"] = "legacy",
            ["OrchardCore_Test:Size"] = "10",
            ["OrchardCore_Test:Nested:Value"] = "legacy-nested",
            ["OrchardCore_Test:Values:0"] = "a",
            ["Test:Name"] = "current",
        });

        var options = configuration.GetSectionCompat("Test", "OrchardCore_Test").Get<TestOptions>();

        Assert.Equal("current", options.Name);
        Assert.Equal(10, options.Size);
        Assert.Equal("legacy-nested", options.Nested.Value);
        Assert.Equal("a", Assert.Single(options.Values));
    }

    [Fact]
    public void GetSectionCompat_ApplicationConfiguration_ResolvesRootedKeys()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore:OrchardCore_KeyVault_Azure:KeyVaultName"] = "legacy",
                ["OrchardCore:KeyVault:Azure:ReloadInterval"] = "60",
            })
            .Build();

        var section = configuration.GetSectionCompat("OrchardCore:KeyVault:Azure", "OrchardCore:OrchardCore_KeyVault_Azure");

        Assert.Equal("legacy", section["KeyVaultName"]);
        Assert.Equal("60", section["ReloadInterval"]);
    }

    [Fact]
    public void GetSectionCompat_SetValue_SetsSectionValue()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Test:Name"] = "legacy",
        });

        var section = configuration.GetSectionCompat("Test", "OrchardCore_Test");
        section["Name"] = "updated";

        Assert.Equal("updated", configuration["Test:Name"]);
        Assert.Equal("legacy", configuration["OrchardCore_Test:Name"]);
    }

    [Fact]
    public void GetSectionCompat_Bind_SectionArrayReplacesLegacyArray()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Test:Values:0"] = "a",
            ["OrchardCore_Test:Values:1"] = "b",
            ["OrchardCore_Test:Values:2"] = "c",
            ["Test:Values:0"] = "x",
        });

        var options = configuration.GetSectionCompat("Test", "OrchardCore_Test").Get<TestOptions>();

        Assert.Equal("x", Assert.Single(options.Values));
    }

    [Fact]
    public void GetSectionCompat_Bind_SectionArrayOfObjectsReplacesLegacyArray()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Test:Items:0:Value"] = "legacy-0",
            ["OrchardCore_Test:Items:0:Other"] = "legacy-other",
            ["OrchardCore_Test:Items:1:Value"] = "legacy-1",
            ["Test:Items:0:Value"] = "current-0",
        });

        var options = configuration.GetSectionCompat("Test", "OrchardCore_Test").Get<TestOptions>();

        var item = Assert.Single(options.Items);
        Assert.Equal("current-0", item.Value);
        Assert.Null(item.Other);
    }

    [Fact]
    public void GetSectionCompat_Bind_OnlyLegacyArray_BindsLegacyArray()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["OrchardCore_Test:Values:0"] = "a",
            ["OrchardCore_Test:Values:1"] = "b",
            ["Test:Name"] = "current",
        });

        var options = configuration.GetSectionCompat("Test", "OrchardCore_Test").Get<TestOptions>();

        Assert.Equal("current", options.Name);
        Assert.Equal(["a", "b"], options.Values.AsEnumerable());
    }

    [Theory]
    [InlineData("X:ConsumerKey", "current")]
    [InlineData("OrchardCore_X:ConsumerKey", "legacy")]
    [InlineData("OrchardCore_Twitter:ConsumerKey", "older")]
    public void GetSectionCompat_MultipleLegacyKeys_UsesKeysByPriority(string winningKey, string expected)
    {
        var values = new Dictionary<string, string>
        {
            ["OrchardCore_Twitter:ConsumerKey"] = "older",
            ["OrchardCore_Twitter:ConsumerSecret"] = "older-secret",
        };

        if (winningKey != "OrchardCore_Twitter:ConsumerKey")
        {
            values["OrchardCore_X:ConsumerKey"] = "legacy";
        }

        if (winningKey == "X:ConsumerKey")
        {
            values["X:ConsumerKey"] = "current";
        }

        var section = CreateConfiguration(values).GetSectionCompat("X", "OrchardCore_X", "OrchardCore_Twitter");

        Assert.Equal(expected, section["ConsumerKey"]);
        Assert.Equal("older-secret", section["ConsumerSecret"]);
    }

    [Fact]
    public void GetSectionCompat_NoLegacyKeys_ReturnsSection()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["Test:Name"] = "current",
        });

        var section = configuration.GetSectionCompat("Test", []);

        Assert.Equal("current", section["Name"]);
    }

    private static ShellConfiguration CreateConfiguration(IDictionary<string, string> values)
        => new(new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build());

    private sealed class TestOptions
    {
        public string Name { get; set; }

        public int Size { get; set; }

        public NestedOptions Nested { get; set; } = new();

        public string[] Values { get; set; } = [];

        public List<ItemOptions> Items { get; set; } = [];
    }

    private sealed class ItemOptions
    {
        public string Value { get; set; }

        public string Other { get; set; }
    }

    private sealed class NestedOptions
    {
        public string Value { get; set; }
    }
}
