using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization;

public interface IPermissionLocalizer
{
    /// <summary>
    /// Localize the description of the given permission
    /// </summary>
    /// <param name="permission"></param>
    /// <returns></returns>
    Permission Localizer(Permission permission);
}
