using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Notifications.Migrations;

public class UserMigrations : DataMigration
{
    public int Create()
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetService<ISession>();
            var notificationMethodProviders = scope.ServiceProvider.GetServices<INotificationMethodProvider>();

            var allNotificationMethods = notificationMethodProviders.Select(x => x.Method).ToArray();

            if (allNotificationMethods.Length == 0)
            {
                return;
            }

            var users = await session.Query<User, UserIndex>(x => x.IsEnabled).ListAsync();

            foreach (var user in users)
            {
                var part = user.As<UserNotificationPart>();

                if (part.Methods == null || !part.Methods.Any())
                {
                    part.Strategy = UserNotificationStrategy.AllMethods;
                    part.Methods = allNotificationMethods;

                    user.Put(part);

                    session.Save(user);
                }
            }
        });

        return 1;
    }
}
