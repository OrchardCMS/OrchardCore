using System;

namespace Microsoft.AspNetCore.Modules
{
    public class Clock : IClock
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}