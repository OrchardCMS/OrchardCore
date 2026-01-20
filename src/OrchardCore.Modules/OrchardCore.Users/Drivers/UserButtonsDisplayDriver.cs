using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers
{
    public class UserButtonsDisplayDriver : DisplayDriver<User>
    {
        public override IDisplayResult Edit(User user)
        {
            return Dynamic("UserSaveButtons_Edit").Location("Actions");
        }

        public override Task<IDisplayResult> UpdateAsync(User user, UpdateEditorContext context)
        {
            return Task.FromResult(Edit(user));
        }
    }
}
