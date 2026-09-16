using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Templates.Endpoints.Management;
using OrchardCore.Templates.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class TemplatesOpenApiDiscoveryTests
{
    [Fact]
    public void Endpoints_ExposeCanonicalTemplatesManagementMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        var app = builder.Build();

        app.AddTemplateManagementEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();

        AssertOperation(endpoints, "api/templates", "GET", "ApiListTemplates", "list", null, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/templates", "POST", "ApiCreateTemplate", "create", "application/json", [201, 400, 401, 403, 409]);
        AssertOperation(endpoints, "api/templates/{name}", "GET", "ApiGetTemplate", "show", null, [200, 401, 403, 404]);
        AssertOperation(endpoints, "api/templates/{name}", "PUT", "ApiUpdateTemplate", "update", "application/json", [200, 400, 401, 403, 404]);
        AssertOperation(endpoints, "api/templates/{name}", "DELETE", "ApiDeleteTemplate", "delete", null, [200, 204, 401, 403]);
    }

    [Fact]
    public async Task Capability_UsesManagementProtocolVersion()
    {
        var capability = Assert.Single(await new TemplatesRemoteManagementCapabilityProvider().GetCapabilitiesAsync());

        Assert.Equal("templates", capability.Id);
        Assert.Equal(
            $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
            capability.Version);
        Assert.Equal("Templates", capability.DisplayName);
    }

    [Fact]
    public void DefinitionSchemaModel_DeclaresRequiredNonEmptyIdentityAndContent()
    {
        AssertRequiredNonEmpty(nameof(TemplateDefinitionDto.Name));
        AssertRequiredNonEmpty(nameof(TemplateDefinitionDto.Content));
        Assert.Null(typeof(TemplateDefinitionDto).GetProperty(nameof(TemplateDefinitionDto.Description))
            ?.GetCustomAttribute<RequiredAttribute>());
    }

    private static void AssertOperation(
        IEnumerable<RouteEndpoint> endpoints,
        string route,
        string method,
        string endpointName,
        string verb,
        string expectedRequestContentType,
        int[] expectedStatuses)
    {
        var endpoint = endpoints.Single(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText, route, StringComparison.Ordinal)
            && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase) == true);

        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));
        Assert.Contains("Templates", endpoint.Metadata.GetRequiredMetadata<ITagsMetadata>().Tags);

        var cli = endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>();
        Assert.Equal(["templates"], cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);
        Assert.Equal("templates", cli.Capability);

        var authorizationPolicy = endpoint.Metadata.GetRequiredMetadata<AuthorizationPolicy>();
        Assert.Contains(OrchardCoreConstants.AuthenticationSchemes.Api, authorizationPolicy.AuthenticationSchemes);
        var permissions = authorizationPolicy.Requirements
            .OfType<PermissionRequirement>()
            .Select(requirement => requirement.Permission)
            .ToArray();
        Assert.Contains(RemoteManagementPermissions.AccessRemoteManagement, permissions);
        Assert.Contains(global::OrchardCore.Templates.Permissions.ManageTemplates, permissions);

        var produces = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Select(metadata => metadata.StatusCode)
            .ToHashSet();
        foreach (var status in expectedStatuses)
        {
            Assert.Contains(status, produces);
        }

        if (expectedRequestContentType is not null)
        {
            var accepts = endpoint.Metadata.GetRequiredMetadata<IAcceptsMetadata>();
            Assert.Contains(expectedRequestContentType, accepts.ContentTypes);
            Assert.Equal(typeof(TemplateDefinitionDto), accepts.RequestType);
            Assert.False(accepts.IsOptional);
        }
    }

    private static void AssertRequiredNonEmpty(string propertyName)
    {
        var property = typeof(TemplateDefinitionDto).GetProperty(propertyName);
        Assert.NotNull(property);
        Assert.NotNull(property.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(1, property.GetCustomAttribute<MinLengthAttribute>()?.Length);
    }
}
