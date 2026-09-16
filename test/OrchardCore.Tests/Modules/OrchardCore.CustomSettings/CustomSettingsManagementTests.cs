using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.CustomSettings;
using OrchardCore.CustomSettings.Endpoints;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Json;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.CustomSettings;

public class CustomSettingsManagementTests
{
    private readonly ClaimsPrincipal _user = new(new ClaimsIdentity("Test"));

    [Fact]
    public async Task ListAsync_FiltersAuthorizationBeforePagingAndUsesStableOrdering()
    {
        var first = CreateDefinition("Zulu", "Alpha display", "find me");
        var denied = CreateDefinition("Denied", "Beta display", "find me");
        var last = CreateDefinition("Alpha", "Zulu display", "find me");
        var fixture = CreateFixture([last, denied, first], deniedNames: ["Denied"]);

        var response = await fixture.Service.ListAsync(_user, "find", -10, 500);

        Assert.Equal(0, response.Skip);
        Assert.Equal(200, response.Take);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(["Zulu", "Alpha"], response.Items.Select(item => item.Name));

        var json = JsonSerializer.SerializeToNode(response)!.AsObject();
        Assert.NotNull(json["skip"]);
        Assert.NotNull(json["take"]);
        Assert.NotNull(json["totalCount"]);
        Assert.NotNull(json["items"]);
        Assert.Null(json["TotalCount"]);
        fixture.SiteService.Verify(service => service.GetSiteSettingsAsync(), Times.Never);
        fixture.SiteService.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Forbidden_DoesNotLoadSiteOrInvokeContentManager()
    {
        var fixture = CreateFixture(
            [CreateDefinition("Protected", "Protected")],
            deniedNames: ["Protected"]);

        var result = await fixture.Service.UpdateAsync(_user, "Protected", []);

        Assert.Equal(CustomSettingsManagementStatus.NotFound, result.Status);
        fixture.SiteService.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
        fixture.SiteService.Verify(service => service.GetSiteSettingsAsync(), Times.Never);
        fixture.ContentManager.Verify(manager => manager.NewAsync(It.IsAny<string>()), Times.Never);
        fixture.ContentManager.Verify(manager => manager.UpdateAsync(It.IsAny<ContentItem>()), Times.Never);
        fixture.ContentManager.Verify(manager => manager.ValidateAsync(It.IsAny<ContentItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ContentHandlerJsonException_ReturnsValidationFailure()
    {
        var fixture = CreateFixture(
            [CreateDefinition("MailSettings", "Mail", partNames: ["MailPart"])]);
        fixture.ContentManager
            .Setup(manager => manager.UpdateAsync(It.IsAny<ContentItem>()))
            .ThrowsAsync(new JsonException("Text must be a string."));

        var result = await fixture.Service.UpdateAsync(
            _user,
            "MailSettings",
            new JsonObject
            {
                ["MailPart"] = new JsonObject
                {
                    ["Text"] = new JsonObject(),
                },
            });

        Assert.Equal(CustomSettingsManagementStatus.ValidationFailed, result.Status);
        Assert.Contains("invalid value", Assert.Single(result.Errors[string.Empty]), StringComparison.OrdinalIgnoreCase);
        fixture.Session.Verify(session => session.CancelAsync(), Times.Once);
        fixture.SiteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ValidMerge_PreservesOmittedValuesAndReplacesArrays()
    {
        var definition = CreateDefinition("MailSettings", "Mail", partNames: ["MailPart", "AuditPart"]);
        var site = CreateSite(new JsonObject
        {
            ["MailSettings"] = new JsonObject
            {
                [nameof(ContentItem.ContentType)] = "MailSettings",
                ["MailPart"] = new JsonObject
                {
                    ["Host"] = "old",
                    ["Port"] = 25,
                    ["Recipients"] = new JsonArray("one", "two"),
                },
                ["AuditPart"] = new JsonObject { ["Enabled"] = true },
            },
        });
        var fixture = CreateFixture([definition], site: site);

        var result = await fixture.Service.UpdateAsync(_user, "MailSettings", new JsonObject
        {
            [nameof(ContentItem.ContentType)] = "MailSettings",
            ["MailPart"] = new JsonObject
            {
                ["Host"] = "new",
                ["Recipients"] = new JsonArray("three"),
            },
        });

        Assert.Equal(CustomSettingsManagementStatus.Success, result.Status);
        Assert.Equal("new", result.Value["MailPart"]!["Host"]!.GetValue<string>());
        Assert.Equal(25, result.Value["MailPart"]!["Port"]!.GetValue<int>());
        Assert.Single(result.Value["MailPart"]!["Recipients"]!.AsArray());
        Assert.True(result.Value["AuditPart"]!["Enabled"]!.GetValue<bool>());
        Assert.Null(result.Value[nameof(ContentItem.ContentItemId)]);
        fixture.ContentManager.Verify(manager => manager.UpdateAsync(It.IsAny<ContentItem>()), Times.Once);
        fixture.ContentManager.Verify(manager => manager.ValidateAsync(It.IsAny<ContentItem>()), Times.Once);
        fixture.Session.Verify(session => session.CancelAsync(), Times.Never);
        fixture.Session.Verify(session => session.Delete(It.IsAny<ContentItem>(), null), Times.Once);
        fixture.SiteService.Verify(service => service.UpdateSiteSettingsAsync(site.Object), Times.Once);
    }

    [Theory]
    [InlineData("ContentItemId")]
    [InlineData("Id")]
    [InlineData("OtherSettings")]
    public async Task UpdateAsync_UnsafeTopLevelProperty_IsRejectedBeforeSiteLoad(string propertyName)
    {
        var fixture = CreateFixture(
            [CreateDefinition("MailSettings", "Mail", partNames: ["MailPart"])]);

        var result = await fixture.Service.UpdateAsync(_user, "MailSettings", new JsonObject
        {
            [propertyName] = "unsafe",
        });

        Assert.Equal(CustomSettingsManagementStatus.ValidationFailed, result.Status);
        Assert.Contains(propertyName, result.Errors);
        fixture.SiteService.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
        fixture.ContentManager.Verify(manager => manager.UpdateAsync(It.IsAny<ContentItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ContentTypeMismatch_IsRejected()
    {
        var fixture = CreateFixture([CreateDefinition("MailSettings", "Mail")]);

        var result = await fixture.Service.UpdateAsync(_user, "MailSettings", new JsonObject
        {
            [nameof(ContentItem.ContentType)] = "OtherSettings",
        });

        Assert.Equal(CustomSettingsManagementStatus.ValidationFailed, result.Status);
        Assert.Contains(nameof(ContentItem.ContentType), result.Errors);
        fixture.SiteService.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ContentValidationFails_DoesNotPersistSiteSettings()
    {
        var validation = new ContentValidateResult();
        validation.Fail(new ValidationResult("Host is required.", ["MailPart.Host"]));
        var fixture = CreateFixture(
            [CreateDefinition("MailSettings", "Mail", partNames: ["MailPart"])],
            validationResult: validation);

        var result = await fixture.Service.UpdateAsync(_user, "MailSettings", new JsonObject
        {
            ["MailPart"] = new JsonObject(),
        });

        Assert.Equal(CustomSettingsManagementStatus.ValidationFailed, result.Status);
        Assert.Equal(["Host is required."], result.Errors["MailPart.Host"]);
        fixture.SiteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Session.Verify(session => session.CancelAsync(), Times.Once);
        fixture.Session.Verify(session => session.Delete(It.IsAny<ContentItem>(), null), Times.Never);
    }

    [Theory]
    [InlineData("""{"MailPart":"invalid"}""", "MailPart")]
    [InlineData("""{"MailPart":null}""", "MailPart")]
    [InlineData("""{"MailPart":{"MailField":"invalid"}}""", "MailPart.MailField")]
    [InlineData("""{"MailPart":{"MailField":null}}""", "MailPart.MailField")]
    public async Task UpdateAsync_InvalidPartOrFieldShape_IsRejected(
        string json,
        string errorKey)
    {
        var definition = CreateDefinition(
            "MailSettings",
            "Mail",
            partNames: ["MailPart"],
            fieldNames: ["MailField"]);
        var fixture = CreateFixture([definition]);

        var result = await fixture.Service.UpdateAsync(
            _user,
            "MailSettings",
            JsonNode.Parse(json)!.AsObject());

        Assert.Equal(CustomSettingsManagementStatus.ValidationFailed, result.Status);
        Assert.Contains(errorKey, result.Errors);
        fixture.ContentManager.Verify(manager => manager.UpdateAsync(It.IsAny<ContentItem>()), Times.Never);
        fixture.SiteService.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CaseVariantRoute_UsesCanonicalResourceName()
    {
        var definition = CreateDefinition("MailSettings", "Mail", partNames: ["MailPart"]);
        var site = CreateSite();
        var fixture = CreateFixture([definition], site: site);

        var result = await fixture.Service.UpdateAsync(
            _user,
            "mailsettings",
            new JsonObject
            {
                [nameof(ContentItem.ContentType)] = "MailSettings",
                ["MailPart"] = new JsonObject { ["Host"] = "localhost" },
            });

        Assert.Equal(CustomSettingsManagementStatus.Success, result.Status);
        Assert.Equal("MailSettings", result.Value[nameof(ContentItem.ContentType)]!.GetValue<string>());
        Assert.True(site.Object.Properties.ContainsKey("MailSettings"));
        Assert.False(site.Object.Properties.ContainsKey("mailsettings"));
    }

    [Fact]
    public async Task UpdateAsync_IdenticalRetries_ReturnSameSafeEnvelope()
    {
        var definition = CreateDefinition("MailSettings", "Mail", partNames: ["MailPart"]);
        var site = CreateSite();
        var fixture = CreateFixture([definition], site: site);
        var input = new JsonObject
        {
            ["MailPart"] = new JsonObject { ["Host"] = "localhost" },
        };

        var first = await fixture.Service.UpdateAsync(_user, "MailSettings", input);
        var second = await fixture.Service.UpdateAsync(_user, "MailSettings", input);

        Assert.Equal(CustomSettingsManagementStatus.Success, first.Status);
        Assert.Equal(CustomSettingsManagementStatus.Success, second.Status);
        Assert.True(JsonNode.DeepEquals(first.Value, second.Value));
        fixture.SiteService.Verify(service => service.UpdateSiteSettingsAsync(site.Object), Times.Exactly(2));
    }

    [Fact]
    public async Task GetSchemaAsync_SanitizesEnvelopeAndRetainsDeclaredComposition()
    {
        var definition = CreateDefinition(
            "MailSettings",
            "Mail settings",
            "Configures mail.",
            ["MailPart"]);
        var fixture = CreateFixture([definition]);

        var result = await fixture.Service.GetSchemaAsync(_user, "MailSettings");

        Assert.Equal(CustomSettingsManagementStatus.Success, result.Status);
        Assert.Equal("Mail settings", result.Value["title"]!.GetValue<string>());
        Assert.Equal("Configures mail.", result.Value["description"]!.GetValue<string>());
        Assert.False(result.Value["additionalProperties"]!.GetValue<bool>());
        Assert.Equal(
            "MailSettings",
            result.Value["properties"]![nameof(ContentItem.ContentType)]!["const"]!.GetValue<string>());
        Assert.NotNull(result.Value["properties"]!["MailPart"]);
        Assert.Null(result.Value["properties"]![nameof(ContentItem.ContentItemId)]);
        Assert.Null(result.Value["properties"]![nameof(ContentItem.Owner)]);
        Assert.Empty(result.Value["required"]!.AsArray());
        fixture.SiteService.Verify(service => service.GetSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task GetSchemaSectionsAsync_OmitsUnauthorizedDefinitionsWithoutLoadingSettings()
    {
        var allowed = CreateDefinition("AllowedSettings", "Allowed", partNames: ["AllowedPart"]);
        var denied = CreateDefinition("DeniedSettings", "Denied", partNames: ["DeniedPart"]);
        var fixture = CreateFixture([denied, allowed], deniedNames: ["DeniedSettings"]);

        var sections = (await fixture.Service.GetSchemaSectionsAsync(_user)).ToArray();

        var section = Assert.Single(sections);
        Assert.Equal("AllowedSettings", section.Name);
        Assert.NotNull(section.Schema["properties"]?["AllowedPart"]);
        fixture.SiteService.Verify(service => service.GetSiteSettingsAsync(), Times.Never);
        fixture.SiteService.Verify(service => service.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task CapabilityAndEndpoints_ExposeStableRemoteManagementMetadata()
    {
        var capability = Assert.Single(
            await new CustomSettingsRemoteManagementCapabilityProvider().GetCapabilitiesAsync());
        Assert.Equal("custom-settings", capability.Id);

        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        app.AddCustomSettingsManagementEndpoints();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .ToArray();
        var operations = endpoints
            .Select(endpoint => endpoint.Metadata.GetMetadata<CliOperationMetadata>())
            .Where(metadata => metadata is not null)
            .ToDictionary(metadata => metadata.Verb);

        Assert.Equal(["list", "schema", "show", "update"], operations.Keys.Order());
        Assert.All(operations.Values, operation => Assert.Equal("custom-settings", operation.Capability));
        Assert.Empty(operations["list"].Arguments);
        Assert.Equal("name", Assert.Single(operations["show"].Arguments).ParameterName);
        Assert.Equal(CliInputMode.Json, operations["update"].InputMode);
        Assert.Equal(3, operations["list"].TableColumns.Count);

        var updateEndpoint = endpoints.Single(endpoint =>
            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "ApiUpdateCustomSettings");
        Assert.NotNull(updateEndpoint.Metadata.GetMetadata<IAcceptsMetadata>());
        Assert.NotNull(updateEndpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>());
        Assert.NotNull(updateEndpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>());
        Assert.NotEmpty(updateEndpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
        Assert.Contains(updateEndpoint.Metadata, metadata =>
            metadata.GetType().Name.Contains("Antiforgery", StringComparison.Ordinal));
    }

    private CustomSettingsManagementFixture CreateFixture(
        ContentTypeDefinition[] definitions,
        string[] deniedNames = null,
        Mock<ISite> site = null,
        ContentValidateResult validationResult = null)
    {
        deniedNames ??= [];
        site ??= CreateSite();
        validationResult ??= new ContentValidateResult();

        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager.Setup(manager => manager.ListTypeDefinitionsAsync()).ReturnsAsync(definitions);
        definitionManager.Setup(manager => manager.LoadTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => definitions.SingleOrDefault(definition =>
                definition.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                _user,
                null,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((
                ClaimsPrincipal _,
                object _,
                IEnumerable<IAuthorizationRequirement> requirements) =>
            {
                var permission = Assert.Single(requirements.OfType<PermissionRequirement>()).Permission;
                var name = permission.Name["ManageCustomSettings_".Length..];
                return deniedNames.Contains(name, StringComparer.Ordinal)
                    ? AuthorizationResult.Failed()
                    : AuthorizationResult.Success();
            });

        var contentManager = new Mock<IContentManager>();
        contentManager.Setup(manager => manager.NewAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new ContentItem { ContentType = name });
        contentManager.Setup(manager => manager.UpdateAsync(It.IsAny<ContentItem>()))
            .Returns(Task.CompletedTask);
        contentManager.Setup(manager => manager.ValidateAsync(It.IsAny<ContentItem>()))
            .ReturnsAsync(validationResult);

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site.Object);
        siteService.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site.Object);
        siteService.Setup(service => service.UpdateSiteSettingsAsync(site.Object))
            .Returns(Task.CompletedTask);

        var serializerOptions = new DocumentJsonSerializerOptions();
        serializerOptions.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
        var session = new Mock<YesSql.ISession>();
        var service = new CustomSettingsManagementService(
            definitionManager.Object,
            siteService.Object,
            contentManager.Object,
            authorizationService.Object,
            session.Object,
            Options.Create(new ContentOptions()),
            Options.Create(serializerOptions));

        return new CustomSettingsManagementFixture(service, siteService, contentManager, session);
    }

    private static ContentTypeDefinition CreateDefinition(
        string name,
        string displayName,
        string description = null,
        string[] partNames = null,
        string[] fieldNames = null)
    {
        var settings = new JsonObject
        {
            [nameof(ContentTypeSettings)] = JsonSerializer.SerializeToNode(new ContentTypeSettings
            {
                Stereotype = CustomSettingsConstants.Stereotype,
                Description = description,
            }),
        };
        var parts = (partNames ?? [])
            .Select(partName => new ContentTypePartDefinition(
                partName,
                new ContentPartDefinition(
                    partName,
                    (fieldNames ?? []).Select(fieldName => new ContentPartFieldDefinition(
                        new ContentFieldDefinition("TextField"),
                        fieldName,
                        [])).ToArray(),
                    []),
                []))
            .ToArray();

        return new ContentTypeDefinition(name, displayName, parts, settings);
    }

    private static Mock<ISite> CreateSite(JsonObject properties = null)
    {
        var site = new Mock<ISite>();
        site.SetupGet(value => value.Properties).Returns(properties ?? []);
        return site;
    }

    private sealed record CustomSettingsManagementFixture(
        CustomSettingsManagementService Service,
        Mock<ISiteService> SiteService,
        Mock<IContentManager> ContentManager,
        Mock<YesSql.ISession> Session);
}
