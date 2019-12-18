using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Users
{
    public class SerializableClaim
    {
        public string Subject { get; set; }

        public string Issuer { get; set; }

        public string OriginalIssuer { get; set; }

        public KeyValuePair<string,string>[] Properties { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string ValueType { get; set; }
    }
}
