using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class AccountActivatedEventDisplay : ActivityDisplayDriver<AccountActivatedEvent, AccountActivatedEventViewModel>
    {
        protected override void EditActivity(AccountActivatedEvent source, AccountActivatedEventViewModel model)
        {
            model.PropertyName = source.PropertyName;
        }

        protected override void UpdateActivity(AccountActivatedEventViewModel model, AccountActivatedEvent activity)
        {
            activity.PropertyName = model.PropertyName.Trim();
        }

        public override IDisplayResult Display(AccountActivatedEvent activity)
        {
            return Combine(
                Shape($"{nameof(AccountActivatedEvent)}_Fields_Thumbnail", new AccountActivatedEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory($"{nameof(AccountActivatedEvent)}_Fields_Design", ctx =>
                {
                    var shape = new AccountActivatedEventViewModel();
                    shape.Activity = activity;

                    return shape;

                }).Location("Design", "Content")
            );
        }
    }
}
