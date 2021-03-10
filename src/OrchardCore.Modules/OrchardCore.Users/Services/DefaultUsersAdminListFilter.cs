using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class DefaultUsersAdminListFilter : IUsersAdminListFilter
    {
        private readonly YesSql.ISession _session;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultUsersAdminListFilter(
            YesSql.ISession session,
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _userManager = userManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task FilterAsync(UserIndexOptions options, IQuery<User> query, IUpdateModel updater)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var userNameIdentifier = user.FindFirstValue(ClaimTypes.NameIdentifier);

            switch (options.Filter)
            {
                case UsersFilter.Enabled:
                    query.With<UserIndex>(u => u.IsEnabled);
                    break;
                case UsersFilter.Disabled:
                    query.With<UserIndex>(u => !u.IsEnabled);
                    break;
                case UsersFilter.Approved:
                    //users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    //users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    //users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var normalizedSearchUserName = _userManager.NormalizeName(options.Search);
                var normalizedSearchEMail = _userManager.NormalizeEmail(options.Search);

                query.With<UserIndex>().Where(u => u.NormalizedUserName.Contains(normalizedSearchUserName) || u.NormalizedEmail.Contains(normalizedSearchEMail));
            }

            // Apply OrderBy filters.
            switch (options.Order)
            {
                case UsersOrder.Name:
                    query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                    break;
                case UsersOrder.Email:
                    query.With<UserIndex>().OrderBy(u => u.NormalizedEmail);
                    break;
                    // case UsersOrder.CreatedUtc:
                    //     //users = users.OrderBy(u => u.CreatedUtc);
                    //     break;
                    // case UsersOrder.LastLoginUtc:
                    //     //users = users.OrderBy(u => u.LastLoginUtc);
                    //     break;
            }

            if (!String.IsNullOrEmpty(options.SelectedRole))
            {
                if (!String.Equals(options.SelectedRole, "Authenticated", StringComparison.OrdinalIgnoreCase))
                {
                    query.With<UserByRoleNameIndex>(x => x.RoleName == options.SelectedRole);
                }
                else
                {

                    /* Sample code for Sqlite to reduce the results, based on no values in the reduce index table(s).

                    SELECT DISTINCT [Document].*, [UserIndex_a1].[NormalizedUserName] 
                    FROM [Document] 
                    INNER JOIN [UserIndex] AS [UserIndex_a1] ON [UserIndex_a1].[DocumentId] = [Document].[Id] 
                    WHERE [Document].[Type] = 'OrchardCore.Users.Models.User, OrchardCore.Users.Core' 
                    AND not exists 
                        (
                            SELECT [RoleName] 
                            FROM UserByRoleNameIndex 
                            INNER JOIN [UserByRoleNameIndex_Document] ON [UserByRoleNameIndex].[Id] = [UserByRoleNameIndex_Document].[UserByRoleNameIndexId] 
                            WHERE [UserByRoleNameIndex_Document].[DocumentId] = [Document].[Id]
                        ) 
                    ORDER BY [UserIndex_a1].[NormalizedUserName] LIMIT 10 OFFSET 0
                    */

                    var dialect = _session.Store.Configuration.SqlDialect;
                    var indexType = typeof(UserByRoleNameIndex);
                    var indexTable = _session.Store.Configuration.TableNameConvention.GetIndexTable(indexType);
                    var documentTable = _session.Store.Configuration.TableNameConvention.GetDocumentTable();
                    var bridgeTableName = indexTable + "_" + documentTable;
                    var bridgeTableIndexColumnName = indexType.Name + "Id";

                    var sqlBuilder = dialect.CreateBuilder(_session.Store.Configuration.TablePrefix);

                    sqlBuilder.Select();
                    sqlBuilder.AddSelector(dialect.QuoteForColumnName("RoleName"));
                    sqlBuilder.From(indexTable);
                    sqlBuilder.InnerJoin(bridgeTableName, indexTable, "Id", bridgeTableName, bridgeTableIndexColumnName);
                    sqlBuilder.WhereAnd($"{dialect.QuoteForTableName(bridgeTableName)}.{dialect.QuoteForColumnName("DocumentId")} = {dialect.QuoteForTableName(documentTable)}.{dialect.QuoteForColumnName("Id")}");

                    var sqlCmd = sqlBuilder.ToSqlString();

                    query.With<UserIndex>().Where($"not exists ({sqlCmd})");
                }
            }

            return Task.CompletedTask;
        }
    }
}
