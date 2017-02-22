using Orchard.Services;
using System;

namespace Orchard.Hosting.Services
{
    public class Clock : IClock
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }
}