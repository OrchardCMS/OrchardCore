using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.ViewModels
{
    public class ConfirmEmailViewModel : ShapeViewModel
    {
        public ConfirmEmailViewModel()
        {
            Metadata.Type = "TemplateUserConfirmEmail";
        }

        public IUser User { get; set; }
        public string ConfirmEmailUrl { get; set; }
    }
}
