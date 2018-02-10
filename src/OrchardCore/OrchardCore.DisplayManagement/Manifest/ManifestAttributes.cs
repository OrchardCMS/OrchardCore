using System;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.DisplayManagement.Manifest
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ThemeAttribute : ModuleAttribute
    {
        public ThemeAttribute(
            string Name = null,
            string Author = "",
            string Website = "",
            string Version = "0.0",
            string Tags = "",
            string Description = "",
            string Dependencies = "",
            string Priority = "0",
            string Category = null,
            string BaseTheme = "")
            : base(Name, Author, Website, Version, Tags, Description, Dependencies, Priority, Category)
        {
            baseTheme = BaseTheme;
        }

        public override string Type => "Theme";
        public string baseTheme { get; }
    }
}