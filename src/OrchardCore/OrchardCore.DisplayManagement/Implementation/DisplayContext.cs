using System;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public DisplayContext()
        {

        }

        public IServiceProvider ServiceProvider { get; set; }
        public IDisplayHelper DisplayAsync { get; set; }
        public object Value { get; set; }
    }
}