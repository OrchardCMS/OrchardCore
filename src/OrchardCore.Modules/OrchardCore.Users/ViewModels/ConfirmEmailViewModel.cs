using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.ViewModels
{
    public class ConfirmEmailViewModel : ShapeViewModel
    {
        public ConfirmEmailViewModel()
             : base("TemplateUserConfirmEmail")
        {
        }

        public IUser User { get; set; }
        public string ConfirmEmailUrl { get; set; }
    }
}
