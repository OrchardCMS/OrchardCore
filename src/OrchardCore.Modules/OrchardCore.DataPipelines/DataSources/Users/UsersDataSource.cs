using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.Users;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.DataSources.Users;

/// <summary>
/// Exposes the users of the site, and their roles, to users allowed to view users. Passwords, security stamps and
/// other secrets are never exposed.
/// </summary>
public sealed class UsersDataSource : IDataSource
{
    /// <summary>
    /// The technical name of the data source.
    /// </summary>
    public const string SourceName = "Users";

    /// <summary>
    /// The data set of the users.
    /// </summary>
    public const string UsersDataSet = "Users";

    /// <summary>
    /// The data set with one row per role of each user.
    /// </summary>
    public const string UserRolesDataSet = "UserRoles";

    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer S;

    public UsersDataSource(
        ISession session,
        IAuthorizationService authorizationService,
        IStringLocalizer<UsersDataSource> localizer)
    {
        _session = session;
        _authorizationService = authorizationService;
        S = localizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Users"];

    public LocalizedString Description => S["The users of the site and their roles."];

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!await CanReadAsync(context))
        {
            return [];
        }

        return [DescribeUsers(), DescribeUserRoles()];
    }

    public async Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!await CanReadAsync(context))
        {
            return null;
        }

        return dataSet switch
        {
            UsersDataSet => new DataSetSchema { DataSet = DescribeUsers(), Fields = UserColumns().Select(column => column.Field).ToList() },
            UserRolesDataSet => new DataSetSchema { DataSet = DescribeUserRoles(), Fields = [.. UserRoleFields()] },
            _ => null,
        };
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.DataSet is not (UsersDataSet or UserRolesDataSet) || !await CanReadAsync(query.Context))
        {
            yield break;
        }

        var columns = UserColumns();
        var roleFields = UserRoleFields();
        var batchSize = Math.Max(1, query.BatchSize);
        var remaining = query.MaxRows > 0 ? query.MaxRows : long.MaxValue;
        var userIds = GetKeys(query, "UserId");
        var lastDocumentId = 0L;

        while (remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var after = lastDocumentId;
            var usersQuery = _session.Query<User, UserIndex>(index => index.DocumentId > after);

            if (userIds is not null)
            {
                usersQuery = usersQuery.Where(index => index.UserId.IsIn(userIds));
            }

            var users = (await usersQuery.OrderBy(index => index.DocumentId).Take(batchSize).ListAsync(cancellationToken)).ToList();

            if (users.Count == 0)
            {
                yield break;
            }

            lastDocumentId = users[^1].Id;

            var rows = new List<object[]>();

            foreach (var user in users)
            {
                if (query.DataSet == UsersDataSet)
                {
                    rows.Add(columns.Select(column => column.Read(user)).ToArray());
                }
                else
                {
                    rows.AddRange(user.RoleNames.Select(role => new object[] { user.UserId, user.UserName, role }));
                }
            }

            if (rows.Count > remaining)
            {
                rows = rows.Take((int)remaining).ToList();
            }

            remaining -= rows.Count;
            _session.Detach(users);

            if (rows.Count > 0)
            {
                yield return new DataBatch(query.DataSet == UsersDataSet ? columns.Select(column => column.Field).ToArray() : roleFields, rows);
            }

            if (users.Count < batchSize)
            {
                yield break;
            }
        }
    }

    private static string[] GetKeys(DataSourceQuery query, string field)
        => query.Conditions?
            .FirstOrDefault(condition => condition.Field == field && condition.Operator is DataFilterOperator.In or DataFilterOperator.Equals)?
            .Values?
            .OfType<string>()
            .ToArray();

    private async Task<bool> CanReadAsync(DataSourceContext context)
        => context?.User is not null && await _authorizationService.AuthorizeAsync(context.User, UsersPermissions.ViewUsers);

    private DataSetDescriptor DescribeUsers()
        => new(UsersDataSet, S["Users"], S["One row per user."]);

    private DataSetDescriptor DescribeUserRoles()
        => new(UserRolesDataSet, S["User roles"], S["One row per role of each user."])
        {
            References = [new(SourceName, UsersDataSet, "UserId")],
        };

    private List<UserColumn> UserColumns()
        =>
        [
            new(new DataField("UserId", S["User id"], DataFieldType.Text) { IsIdentifier = true, IsKeyFilterable = true }, user => user.UserId),
            new(new DataField("UserName", S["User name"], DataFieldType.Text), user => user.UserName),
            new(new DataField("Email", S["Email"], DataFieldType.Text), user => user.Email),
            new(new DataField("EmailConfirmed", S["Email confirmed"], DataFieldType.Boolean), user => user.EmailConfirmed),
            new(new DataField("PhoneNumber", S["Phone number"], DataFieldType.Text), user => user.PhoneNumber),
            new(new DataField("PhoneNumberConfirmed", S["Phone number confirmed"], DataFieldType.Boolean), user => user.PhoneNumberConfirmed),
            new(new DataField("IsEnabled", S["Enabled"], DataFieldType.Boolean), user => user.IsEnabled),
            new(new DataField("TwoFactorEnabled", S["Two-factor authentication"], DataFieldType.Boolean), user => user.TwoFactorEnabled),
            new(new DataField("IsLockoutEnabled", S["Lockout enabled"], DataFieldType.Boolean), user => user.IsLockoutEnabled),
            new(new DataField("LockoutEndUtc", S["Locked out until"], DataFieldType.DateTime), user => user.LockoutEndUtc),
            new(new DataField("AccessFailedCount", S["Failed sign-ins"], DataFieldType.Integer), user => (long)user.AccessFailedCount),
            new(new DataField("Roles", S["Roles"], DataFieldType.Text), user => user.RoleNames.Count == 0 ? null : string.Join(',', user.RoleNames)),
        ];

    private DataField[] UserRoleFields()
        =>
        [
            new("UserId", S["User id"], DataFieldType.Text) { IsIdentifier = true, References = [new(SourceName, UsersDataSet, "UserId")] },
            new("UserName", S["User name"], DataFieldType.Text),
            new("RoleName", S["Role"], DataFieldType.Text),
        ];

    private sealed record UserColumn(DataField Field, Func<User, object> Read);
}
