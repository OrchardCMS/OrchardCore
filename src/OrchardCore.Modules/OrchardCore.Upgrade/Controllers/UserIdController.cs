using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Security;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Upgrade.ViewModels;
using YesSql;
using OrchardCore.Settings;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;

namespace OrchardCore.Upgrade.Controllers
{
    [Feature("OrchardCore.Upgrade.UserId")]
    [Admin]
    public class UserIdController : Controller
    {
        private const int UserBatchSize = 50;
        private const int ContentItemBatchSize = 100;

        private readonly IUserIdGenerator _userIdGenerator;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly UserManager<IUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public UserIdController(
            IUserIdGenerator userIdGenerator,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            UserManager<IUser> userManager,
            ISession session)
        {
            _userIdGenerator = userIdGenerator;
            _session = session;
            _siteService = siteService;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexPost(UserIdUpgradeViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, StandardPermissions.SiteOwner))
            {
                return this.ChallengeOrForbid();
            }

            // Process one user at a time as the batching needs to be done on content items.
            // e.g. one user (admin) may own thousands of content items.

            User currentUser = null;
            if (model.UserDocumentId == 0)
            {
                // Find first user with no UserId
                currentUser = await _session.Query<User>()
                    .With<UserIndex>(x => x.UserId == null)
                    .OrderBy(x => x.DocumentId)
                    .FirstOrDefaultAsync();

                if (currentUser == null)
                {
                    // This can occur if the upgrade is run on a site twice.
                    model.Complete = true;
                    return Json(model);
                }

                // Update and persist the user id.
                // If the migration is stopped partway through this user a new migration
                // will reset this id, and restart updating content items all with the new id.
                currentUser.UserId = _userIdGenerator.GenerateUniqueId(currentUser);
                _session.Save(currentUser);
            }
            else
            {
                currentUser = await _session.Query<User>()
                    .With<UserIndex>()
                    .Where(x => x.DocumentId == model.UserDocumentId)
                    .FirstOrDefaultAsync();
            }


            var contentItems = await _session.Query<ContentItem>()
                .With<ContentItemIndex>(x => x.OwnerId != currentUser.UserId && x.Owner == currentUser.UserName)
                .OrderBy(x => x.DocumentId) // This is not required but useful to stay sequential.
                .Take(ContentItemBatchSize)
                .ListAsync();

            if (!contentItems.Any())
            {
                // Processing content items for this user is complete, find the next user.
                var nextUser =  await _session.Query<User>()
                    .With<UserIndex>(x => x.UserId == null)
                    .OrderBy(x => x.DocumentId)
                    .FirstOrDefaultAsync();

                if (nextUser == null)
                {
                    // Entire operation is complete.
                    model.Complete = true;
                    // Update the site owner.
                    var siteSettings = await _siteService.LoadSiteSettingsAsync();
                    // Use user manager here as it handles normalization.
                    var user = await _userManager.FindByNameAsync(siteSettings.SuperUser);
                    siteSettings.SuperUserId = user.UserId;
                    await _siteService.UpdateSiteSettingsAsync(siteSettings);
                }
                else
                {
                    model.UserDocumentId = nextUser.Id;
                    // Apply a new id to the next user.
                    nextUser.UserId = _userIdGenerator.GenerateUniqueId(currentUser);
                    _session.Save(currentUser);
                }

                return Json(model);
            }

            foreach(var contentItem in contentItems)
            {
                contentItem.OwnerId = currentUser.UserId;
                _session.Save(contentItem);
                model.ContentItemCounter++;
            }

            model.UserDocumentId = currentUser.Id;

            return Json(model);
        }
    }
}
