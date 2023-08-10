using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

#pragma warning disable CA1050 // Declare types in namespaces
public static class UserRazorHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a <see cref="User"/> by its <see cref="User.UserId"/>
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="userId">The <see cref="User.UserId"/>.</param>
    /// <returns>A <see cref="User"/> or <c>null</c> if it was not found.</returns>
    public static Task<User> GetUserByIdAsync(this IOrchardHelper orchardHelper, string userId)
    {
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();
        return session.Query<User, UserIndex>(x => x.UserId == userId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Loads a list of users by their user ids./>
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="userIds">The user ids to load.</param>
    /// <returns>A list of users with the specific ids.</returns>
    public static Task<IEnumerable<User>> GetUsersByIdsAsync(this IOrchardHelper orchardHelper, IEnumerable<string> userIds)
    {
        var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();
        return session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync();
    }
}
