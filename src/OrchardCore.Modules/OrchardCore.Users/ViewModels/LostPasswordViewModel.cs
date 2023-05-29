using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.ViewModels
{
    public class LostPasswordViewModel : ShapeViewModel
    {
        public LostPasswordViewModel()
            : base("TemplateUserLostPassword")
        {
        }

        public User User { get; set; }
        public string LostPasswordUrl { get; set; }
    }
}
