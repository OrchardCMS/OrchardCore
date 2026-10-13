using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.OptionsMonitoring;

/// <summary>
/// Helpers to invoke the settings display drivers that request options updates.
/// </summary>
internal static class SettingsDriverTestHelper
{
    public static IAuthorizationService AuthorizeAll()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService.Object;
    }

    public static IHttpContextAccessor HttpContextAccessor()
        => new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

    /// <summary>
    /// Creates an update context binding the specified model, with a model error if it should be invalid.
    /// </summary>
    public static UpdateEditorContext CreateUpdateContext<TModel>(TModel model, string groupId, bool isValid = true)
        where TModel : class
    {
        var updater = new StubModelUpdater<TModel>(model);
        if (!isValid)
        {
            updater.ModelState.AddModelError(string.Empty, "Invalid");
        }

        return new UpdateEditorContext(new Shape(), groupId, false, string.Empty, null, null, updater);
    }
}
