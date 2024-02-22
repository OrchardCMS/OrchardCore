using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Users.GraphQL;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentFields.GraphQL
{
    public class UserPickerFieldQueryObjectType : ObjectGraphType<UserPickerField>
    {
        public UserPickerFieldQueryObjectType(UserType userType)
        {
            Name = nameof(UserPickerField);

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>("userIds")
                .Description("user ids")
                .PagingArguments()
                .Resolve(x =>
                {
                    return x.Page(x.Source.UserIds);
                });

            Field<ListGraphType<UserType>, IEnumerable<User>>("users")
                .Type(new ListGraphType(userType))
                .Description("the user items")
                .PagingArguments()
                .ResolveAsync(x =>
                {
                    var userLoader = GetOrAddUserProfileByIdDataLoader(x);
                    return userLoader.LoadAsync(x.Page(x.Source.UserIds)).Then(itemResultSet =>
                    {
                        return itemResultSet.SelectMany(x => x);
                    });
                });
            Field<UserType, User>("user")
                .Type(userType)
                .Description("the first user")
                .ResolveAsync(x =>
                {
                    var userLoader = GetOrAddUserProfileByIdDataLoader(x);
                    return userLoader.LoadAsync(x.Source.UserIds.FirstOrDefault()).Then(itemResultSet =>
                    {
                        return itemResultSet.FirstOrDefault();
                    });
                });
        }

        public static IDataLoader<string, IEnumerable<User>> GetOrAddUserProfileByIdDataLoader<T>(IResolveFieldContext<T> context)
        {
            IDataLoaderContextAccessor requiredService = context.RequestServices.GetRequiredService<IDataLoaderContextAccessor>();
            var session = context.RequestServices.GetService<ISession>();
            return requiredService.Context.GetOrAddCollectionBatchLoader("GetOrAddUserByIds", async (IEnumerable<string> userIds) =>
            {
                if (userIds == null || !userIds.Any())
                {
                    return null;
                }
                var users = await session.Query<User, UserIndex>(y => y.UserId.IsIn(userIds)).ListAsync();

                return users.ToLookup((User k) => k.UserId, (User user) => user);
            });
        }
    }
}
