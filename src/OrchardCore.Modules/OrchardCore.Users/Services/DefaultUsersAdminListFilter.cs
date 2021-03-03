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
using YesSql.Services;

namespace OrchardCore.Users.Services
{
    public class DefaultUsersAdminListFilter : IUsersAdminListFilter
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultUsersAdminListFilter(
            UserManager<IUser> userManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
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

                query = query.With<UserIndex>().Where(u => u.NormalizedUserName.Contains(normalizedSearchUserName) || u.NormalizedEmail.Contains(normalizedSearchEMail));
            }

            // Apply OrderBy filters.
            switch (options.Order)
            {
                case UsersOrder.Name:
                    query = query.With<UserIndex>().OrderBy(u => u.NormalizedUserName);
                    break;
                case UsersOrder.Email:
                    query = query.With<UserIndex>().OrderBy(u => u.NormalizedEmail);
                    break;
                case UsersOrder.CreatedUtc:
                    //users = users.OrderBy(u => u.CreatedUtc);
                    break;
                case UsersOrder.LastLoginUtc:
                    //users = users.OrderBy(u => u.LastLoginUtc);
                    break;
            }



            // if (!String.IsNullOrEmpty(model.DisplayText))
            // {
            //     query.With<ContentItemIndex>(x => x.DisplayText.Contains(model.DisplayText));
            // }


            return Task.CompletedTask;


        }
    }
}
