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
            DisplayAsync = context.DisplayAsync;
            Value = context.Value;
            HtmlFieldPrefix = context.HtmlFieldPrefix;
        }

        public IServiceProvider ServiceProvider { get; set; }
        public IDisplayHelper DisplayAsync { get; set; }
        public string HtmlFieldPrefix { get; set; }
        public object Value { get; set; }
    }
}
