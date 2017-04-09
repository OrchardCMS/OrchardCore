using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemeViewStart : Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        public override Task ExecuteAsync()
        {
            Layout = "~/Views/Shared/_Layout.cshtml";
            return Task.CompletedTask;
        }
    }
}
