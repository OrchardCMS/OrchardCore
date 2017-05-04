using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Abstractions
{
    public class PluralArgument
    {
        public string[] PluralForms { get; set; }
        public int Count { get; set; }
    }
}
