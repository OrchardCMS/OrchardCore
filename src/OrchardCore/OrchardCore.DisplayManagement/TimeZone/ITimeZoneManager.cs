using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.TimeZone
{
    public interface ITimeZoneManager
    {
        Task<ITimeZone> GetTimeZoneAsync();
    }
}
