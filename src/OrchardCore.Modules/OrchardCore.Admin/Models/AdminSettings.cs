namespace OrchardCore.Admin.Models
{
    public class AdminSettings : IAdminSettings
    {
        public bool DisplayDarkMode { get; set; } = true;

        public bool DisplayMenuFilter { get; set; } = true;
    }
}
