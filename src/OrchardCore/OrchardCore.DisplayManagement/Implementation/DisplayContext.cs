using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IDisplayHelper DisplayAsync { get; set; }
        public ViewContext ViewContext { get; set; }
        public object Value { get; set; }
    }
}