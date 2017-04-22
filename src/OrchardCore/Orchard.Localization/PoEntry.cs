using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization
{
    public class PoEntry
    {
        public string MessageId { get; set; }
        public string Context { get; set; }
        public string[] Values { get; set; }
    }
}
