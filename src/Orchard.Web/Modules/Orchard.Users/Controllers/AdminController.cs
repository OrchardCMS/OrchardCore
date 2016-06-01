using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.Navigation;
using Orchard.Settings;
using Orchard.Users.Indexes;
using Orchard.Users.Models;
using Orchard.Users.ViewModels;
using YesSql.Core.Services;

namespace Orchard.Users.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;

        public AdminController(
            IAuthorizationService authorizationService,
            ISession session,
            UserManager<User> userManager,
            IStringLocalizer<AdminController> stringLocalizer,
            ISiteService siteService,
            IShapeFactory shapeFactory
            )
        {
            _shapeFactory = shapeFactory;
            _siteService = siteService;
            T = stringLocalizer;
            _authorizationService = authorizationService;
            _session = session;
            _userManager = userManager;
        }
        public async Task<ActionResult> Index(UserIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageUsers))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new UserIndexOptions();
            }

            var users = _session.QueryAsync<User, UserIndex>();

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
                users = users.Where(u => u.NormalizedUserName.Contains(options.Search) || u.NormalizedEmail.Contains(options.Search));
            }


            switch (options.Order)
            {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.NormalizedUserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.NormalizedEmail);
                    break;
                case UsersOrder.CreatedUtc:
                    //users = users.OrderBy(u => u.CreatedUtc);
                    break;
                case UsersOrder.LastLoginUtc:
                    //users = users.OrderBy(u => u.LastLoginUtc);
                    break;
            }

            var results = await users
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .List();

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            var pagerShape = _shapeFactory.Create("Pager", new { TotalItemCount = await users.Count(), RouteData = routeData });

            var model = new UsersIndexViewModel
            {
                Users = results
                    .Select(x => new UserEntry { User = x })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }
    }
}
