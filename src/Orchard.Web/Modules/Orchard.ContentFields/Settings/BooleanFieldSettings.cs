using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentFields.Settings
{
    public class BooleanFieldSettings
    {
        public string Hint { get; set; }
        public bool Optional { get; set; }
        public string NotSetLabel { get; set; }
        public string OnLabel { get; set; }
        public string OffLabel { get; set; }
        public SelectionMode SelectionMode { get; set; }
        public bool? DefaultValue { get; set; }

        public BooleanFieldSettings()
        {
            OnLabel = "Yes";
            OffLabel = "No";
            SelectionMode = SelectionMode.Checkbox;
        }
    }

    public enum SelectionMode
    {
        Checkbox,
        Radiobutton,
        Dropdown
    }
}