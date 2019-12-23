using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class AccountActivationEventDisplay : ActivityDisplayDriver<AccountActivationEvent, AccountActivationEventViewModel>
    {
        protected override void EditActivity(AccountActivationEvent source, AccountActivationEventViewModel model)
        {
            model.PropertyName = source.PropertyName;
        }

        protected override void UpdateActivity(AccountActivationEventViewModel model, AccountActivationEvent activity)
        {
            activity.PropertyName = model.PropertyName.Trim();
        }

        public override IDisplayResult Display(AccountActivationEvent activity)
        {
            return Combine(
                Shape($"{nameof(AccountActivationEvent)}_Fields_Thumbnail", new AccountActivationEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory($"{nameof(AccountActivationEvent)}_Fields_Design", ctx =>
                {
                    var shape = new AccountActivationEventViewModel();
                    shape.Activity = activity;

                    return shape;

                }).Location("Design", "Content")
            );
        }
    }
}
