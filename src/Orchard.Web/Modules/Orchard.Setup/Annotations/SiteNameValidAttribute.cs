using Orchard.Localization;

namespace Orchard.Setup.Annotations
{
    public class SiteNameValidAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
    {
        private string _value;

        public SiteNameValidAttribute(int maximumLength)
            : base(1, maximumLength)
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override bool IsValid(object value)
        {
            _value = (value as string) ?? "";
            return base.IsValid(_value.Trim().Length);
        }

        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrWhiteSpace(_value))
                return T("Site name is required.");

            return T("Site name can be no longer than {0} characters.", Maximum);
        }
    }
}