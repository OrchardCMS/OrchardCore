using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement;
using Orchard.Settings;
using YesSql.Core.Services;

namespace Orchard.Roles.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer T;
        private readonly ISiteService _siteService;
        private readonly IShapeFactory _shapeFactory;

        public AdminController(
            IAuthorizationService authorizationService,
            ISession session,
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
        }
        public async Task<ActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageRoles))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            return View();
        }
    }
}
