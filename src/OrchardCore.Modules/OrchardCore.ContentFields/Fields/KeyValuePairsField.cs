using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class KeyValuePairsField : ContentField
    {
        public KeyValuePair<string, string>[] Values { get; set; } = Array.Empty<KeyValuePair<string, string>>();
    }
}
