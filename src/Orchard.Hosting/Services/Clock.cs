using System;

namespace Microsoft.AspNetCore.Modules
{
    public class Clock : IClock
    {
        public DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }
    }
}