using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers
{
    public class UserButtonsDisplayDriver : DisplayDriver<IUser>
    {
        public override IDisplayResult Edit(IUser user)
        {
            return Shape("IUser_SaveButtons").Location("Actions");
        }

        public override Task<IDisplayResult> UpdateAsync(IUser user, UpdateEditorContext context)
        {
            return Task.FromResult(Edit(user));
        }
    }
}
