using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Upgrade.Services
{
    /// <summary>
    /// Intercepts any request to the admin site to notify the user an upgrade is required.
    /// </summary>
    public class UserIdUpgradeFilter : UpgradeFilter
    {
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<UserIdUpgradeFilter> T;

        public UserIdUpgradeFilter(INotifier notifier,
            IHtmlLocalizer<UserIdUpgradeFilter> htmlLocalizer)
        {
            _notifier = notifier;
            T = htmlLocalizer;
        }

        protected override void Notify()
        {
            _notifier.Error(T["An upgrade to the User Id is required before the site can be made operational. Please use the Upgrade User Id Feature."]);
        }
    }
}
