using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Setup.Annotations
{
    public class SiteNameValidAttribute : RangeAttribute
    {
        private string _value;

        public SiteNameValidAttribute(int maximumLength)
            : base(1, maximumLength)
        {
        }

        public override bool IsValid(object value)
        {
            _value = (value as string) ?? String.Empty;

            return base.IsValid(_value.Trim().Length);
        }

        public override string FormatErrorMessage(string name) => String.IsNullOrWhiteSpace(_value)
            ? "Site name is required."
            : $"Site name can be no longer than {Maximum} characters.";
    }
}
