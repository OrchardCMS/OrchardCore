using System;

namespace OrchardCore.Admin.Models
{
    public class AdminThemeSettings
    {
        public string Theme { get; set; } = "light";
        public string BackgroundClass
        {
            get
            {
                return (Theme == "light") ? "white" : "dark";
            }
        }
        public string HeaderClass
        {
            get
            {
                return (Theme == "light") ? "light" : "dark";
            }
        }
        public string TextClass
        {
            get
            {
                return (Theme == "light") ? "dark" : "light";
            }
        }
    }
}
