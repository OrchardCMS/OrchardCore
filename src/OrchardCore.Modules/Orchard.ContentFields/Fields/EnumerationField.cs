using System;
using Orchard.ContentManagement;

namespace Orchard.ContentFields.Fields
{
    public class EnumerationField : ContentField
    {
        private const char Separator = ';';

        public string Value { get; set; }

        public string[] SelectedValues
        {
            get
            {
                var value = Value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new string[0];
                }

                return value.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            }

            set
            {
                if (value == null || value.Length == 0)
                {
                    Value = string.Empty;
                }
                else
                {
                    Value = Separator + string.Join(Separator.ToString(), value) + Separator;
                }
            }
        }
    }
}
