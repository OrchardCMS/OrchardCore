using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Settings
{
    public class TestSettings : ContentPart
    {
        public TextField TestSetting { get; set; }
    }
}
