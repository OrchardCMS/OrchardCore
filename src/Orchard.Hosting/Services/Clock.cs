using Orchard.Services;
using System;

namespace Orchard.Hosting.Services
{
    public class Clock : IClock
    {
        public DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }
    }
}