using System.Collections.Generic;

namespace Orchard.Users
{
    /// <summary>
    /// Marker interface for ASP.NET Core Identity services.
    /// </summary>
    public interface IUser
    {
        string UserName { get; }
        IList<string> RoleNames { get; }
    }
}
