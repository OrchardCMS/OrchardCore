using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers
{

    public class UserDisplayDriver : DisplayDriver<IUser>
    {
        //public override IDisplayResult Edit(IUser user)
        //{
        //    return Shape<User>("User_Edit", model =>
        //    {
        //        model.UserName = user.UserName;
        //        model.Email = user.Email;
        //    }).Location("Content:1");
        //}

        public override Task<IDisplayResult> EditAsync(IUser user, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                    Shape<EditUserViewModel>("User_Edit", model =>
                    {
                        model.UserName = user.UserName;
                        //model.Email = user.Email;
                    }).Location("Content:1")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(IUser user, UpdateEditorContext context)
        {
            await context.Updater.TryUpdateModelAsync(user, Prefix, t => t.UserName/*, t => t.Email*/);

            return Edit(user);
        }
    }
}
