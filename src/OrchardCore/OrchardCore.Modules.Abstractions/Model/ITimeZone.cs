using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Modules
{
    public interface ITimeZone
    {
        string Id { get; set; }
        string DisplayName { get; set; }
        string Comment { get; set; }
    }
}
