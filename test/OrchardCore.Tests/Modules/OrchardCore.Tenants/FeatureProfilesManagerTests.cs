using Microsoft.Extensions.Localization;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class FeatureProfilesManagerTests
{
    [Fact]
    public async Task Update_EquivalentDefinition_DoesNotPersistAgain()
    {
        var document = new FeatureProfilesDocument();
        document.FeatureProfiles["profile"] = Definition();
        var manager = CreateManager(document, out var documents);

        await manager.UpdateFeatureProfileAsync("profile", Definition());

        documents.Verify(store => store.UpdateAsync(It.IsAny<FeatureProfilesDocument>(),
            It.IsAny<Func<FeatureProfilesDocument, Task>>()), Times.Never);
    }

    [Fact]
    public async Task Remove_MissingProfile_DoesNotPersistAgain()
    {
        var manager = CreateManager(new FeatureProfilesDocument(), out var documents);

        await manager.RemoveFeatureProfileAsync("missing");

        documents.Verify(store => store.UpdateAsync(It.IsAny<FeatureProfilesDocument>(),
            It.IsAny<Func<FeatureProfilesDocument, Task>>()), Times.Never);
    }

    [Fact]
    public async Task Update_ChangedRuleOrder_PersistsNewPrecedence()
    {
        var document = new FeatureProfilesDocument();
        document.FeatureProfiles["profile"] = Definition();
        var manager = CreateManager(document, out var documents);
        var replacement = Definition();
        replacement.FeatureRules.Reverse();

        await manager.UpdateFeatureProfileAsync("profile", replacement);

        Assert.Equal("Include", document.FeatureProfiles["profile"].FeatureRules[0].Rule);
        documents.Verify(store => store.UpdateAsync(document,
            It.IsAny<Func<FeatureProfilesDocument, Task>>()), Times.Once);
    }

    [Theory]
    [InlineData("null-rules")]
    [InlineData("unknown-rule")]
    [InlineData("empty-expression")]
    [InlineData("mismatched-id")]
    public async Task Update_InvalidDefinition_PreservesStoredProfile(string invalid)
    {
        var document = new FeatureProfilesDocument();
        var original = Definition();
        document.FeatureProfiles["profile"] = original;
        var manager = CreateManager(document, out var documents);
        var replacement = Definition();
        switch (invalid)
        {
            case "null-rules": replacement.FeatureRules = null; break;
            case "unknown-rule": replacement.FeatureRules[0].Rule = "Unknown"; break;
            case "empty-expression": replacement.FeatureRules[0].Expression = " "; break;
            case "mismatched-id": replacement.Id = "other"; break;
        }

        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(
            () => manager.UpdateFeatureProfileAsync("profile", replacement));

        Assert.Same(original, document.FeatureProfiles["profile"]);
        documents.Verify(store => store.UpdateAsync(It.IsAny<FeatureProfilesDocument>(),
            It.IsAny<Func<FeatureProfilesDocument, Task>>()), Times.Never);
    }

    [Fact]
    public async Task Update_LegacyRecipeDefinition_PreservesKeyFallbackAndRejectsDuplicateName()
    {
        var document = new FeatureProfilesDocument();
        var manager = CreateManager(document, out _);
        await manager.UpdateFeatureProfileAsync("legacy", new FeatureProfile());
        Assert.Null(document.FeatureProfiles["legacy"].Id);
        Assert.Null(document.FeatureProfiles["legacy"].Name);
        var errors = await manager.ValidateFeatureProfileAsync("other", new FeatureProfile { Name = "legacy" });
        Assert.Contains("Name", errors.Keys);
    }

    private static FeatureProfile Definition() => new()
    {
        Id = "profile", Name = "Profile",
        FeatureRules = [new() { Rule = "Exclude", Expression = "Custom.*" },
            new() { Rule = "Include", Expression = "Custom.Allowed" }],
    };

    internal static FeatureProfilesManager CreateManager(FeatureProfilesDocument document,
        out Mock<IDocumentManager<FeatureProfilesDocument>> documents)
    {
        documents = new Mock<IDocumentManager<FeatureProfilesDocument>>();
        documents.Setup(store => store.GetOrCreateMutableAsync(It.IsAny<Func<Task<FeatureProfilesDocument>>>())).ReturnsAsync(document);
        documents.Setup(store => store.UpdateAsync(document, It.IsAny<Func<FeatureProfilesDocument, Task>>())).Returns(Task.CompletedTask);
        documents.Setup(store => store.GetOrCreateImmutableAsync(It.IsAny<Func<Task<FeatureProfilesDocument>>>())).ReturnsAsync(document);
        var rules = new FeatureProfilesRuleOptions();
        rules.Rules["Include"] = (_, _) => (true, true);
        rules.Rules["Exclude"] = (_, _) => (true, false);
        var localizer = new Mock<IStringLocalizer<FeatureProfilesManager>>();
        localizer.Setup(value => value[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));
        return new FeatureProfilesManager(documents.Object, Options.Create(rules), localizer.Object);
    }
}
