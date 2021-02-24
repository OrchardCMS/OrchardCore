using System;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public DisplayContext()
        {
        }

        public DisplayContext(DisplayContext context)
        {
            ServiceProvider = context.ServiceProvider;
            DisplayHelper = context.DisplayHelper;
            Value = context.Value;
            HtmlFieldPrefix = context.HtmlFieldPrefix;
        }

        public IServiceProvider ServiceProvider { get; set; }
        public IDisplayHelper DisplayHelper { get; set; }
        public string HtmlFieldPrefix { get; set; }
        public IShape Value { get; set; }
    }
}
