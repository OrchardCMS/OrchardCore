using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public DisplayHelper DisplayAsync { get; set; }
        public ViewContext ViewContext { get; set; }
        public object Value { get; set; }
    }
}