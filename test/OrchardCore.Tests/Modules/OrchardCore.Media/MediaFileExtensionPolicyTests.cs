using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaFileExtensionPolicyTests
{
    private static readonly ClaimsPrincipal User = new();

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".JPG")]
    public async Task IsAllowedAsync_StandardExtension_AllowsWithoutAdditionalPermission(string extension)
    {
        var policy = CreatePolicy(canUploadRestrictedMedia: false);

        Assert.True(await policy.IsAllowedAsync(User, extension));
    }

    [Fact]
    public async Task IsAllowedAsync_RestrictedExtension_DeniesWithoutAdditionalPermission()
    {
        var policy = CreatePolicy(canUploadRestrictedMedia: false);

        Assert.False(await policy.IsAllowedAsync(User, ".svg"));
    }

    [Theory]
    [InlineData(".svg")]
    [InlineData(".SVG")]
    public async Task IsAllowedAsync_RestrictedExtension_AllowsWithAdditionalPermission(string extension)
    {
        var policy = CreatePolicy(canUploadRestrictedMedia: true);

        Assert.True(await policy.IsAllowedAsync(User, extension));
    }

    [Fact]
    public async Task IsAllowedAsync_UnconfiguredExtension_DeniesWithAdditionalPermission()
    {
        var policy = CreatePolicy(canUploadRestrictedMedia: true);

        Assert.False(await policy.IsAllowedAsync(User, ".exe"));
    }

    [Fact]
    public async Task IsAllowedAsync_OverlappingExtension_RestrictedPermissionWins()
    {
        var options = CreateOptions();
        options.AllowedFileExtensions.Add(".svg");

        var unprivilegedPolicy = CreatePolicy(canUploadRestrictedMedia: false, options);
        var privilegedPolicy = CreatePolicy(canUploadRestrictedMedia: true, options);

        Assert.False(await unprivilegedPolicy.IsAllowedAsync(User, ".svg"));
        Assert.True(await privilegedPolicy.IsAllowedAsync(User, ".svg"));
    }

    [Fact]
    public async Task GetAllowedFileExtensionsAsync_ReturnsEffectiveExtensionsForUser()
    {
        var unprivilegedPolicy = CreatePolicy(canUploadRestrictedMedia: false);
        var privilegedPolicy = CreatePolicy(canUploadRestrictedMedia: true);

        var unprivilegedExtensions = await unprivilegedPolicy.GetAllowedFileExtensionsAsync(User);
        var privilegedExtensions = await privilegedPolicy.GetAllowedFileExtensionsAsync(User);

        Assert.Equal([".jpg"], unprivilegedExtensions);
        Assert.Equal(2, privilegedExtensions.Count);
        Assert.Contains(".jpg", privilegedExtensions);
        Assert.Contains(".svg", privilegedExtensions);
    }

    [Fact]
    public void GetConfiguredFileExtensions_RestrictedExtension_IncludesForTrustedFlows()
    {
        var extensions = MediaFileExtensionPolicy.GetConfiguredFileExtensions(CreateOptions());

        Assert.Equal(2, extensions.Count);
        Assert.Contains(".jpg", extensions);
        Assert.Contains(".svg", extensions);
    }

    [Fact]
    public void Validate_OverlappingExtensions_Fails()
    {
        var options = CreateOptions();
        options.AllowedFileExtensions.Add(".SVG");

        var result = new MediaOptionsValidator().Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(".svg", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static MediaFileExtensionPolicy CreatePolicy(
        bool canUploadRestrictedMedia,
        MediaOptions options = null)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, _, requirements) =>
            {
                var isRestrictedUploadPermission = requirements
                    .OfType<PermissionRequirement>()
                    .Any(requirement => requirement.Permission.Name == MediaPermissions.UploadRestrictedMedia.Name);

                return Task.FromResult(isRestrictedUploadPermission && canUploadRestrictedMedia
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());
            });

        return new MediaFileExtensionPolicy(
            authorizationService.Object,
            Options.Create(options ?? CreateOptions()));
    }

    private static MediaOptions CreateOptions() => new()
    {
        AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg" },
        AllowedFileExtensionsWithPermission = new(StringComparer.OrdinalIgnoreCase) { ".svg" },
    };
}
