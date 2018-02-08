using System;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.DisplayManagement.Manifest
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ThemeAttribute : ModuleAttribute
    {
        public ThemeAttribute(
            string name = null,
            string author = "",
            string website = "",
            string version = "0.0",
            string tags = "",
            string description = "",
            string dependencies = "",
            string priority = "0",
            string category = null,
            string baseTheme = "")
            : base(name, author, website, version, tags, description, dependencies, priority, category)
        {
            BaseTheme = baseTheme;
        }

        public override string Type => "Theme";
        public string BaseTheme { get; }
    }
}