using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class UserButtonsDisplayDriver : DisplayDriver<User>
    {
        public override Task<IDisplayResult> EditAsync(User model, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Dynamic("UserSaveButtons_Edit").Location("Actions")
            );
        }

        public override Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            return EditAsync(user, context);
        }
    }
}
