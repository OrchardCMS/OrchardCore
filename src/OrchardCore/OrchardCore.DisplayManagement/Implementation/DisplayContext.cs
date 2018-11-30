using System;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        // TODO: can it be an IShape
        public object Value { get; set; }
        public string HtmlFieldPrefix { get; set; }
    }
}