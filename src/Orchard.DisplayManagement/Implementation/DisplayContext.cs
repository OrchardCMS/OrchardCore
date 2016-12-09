using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public DisplayHelper DisplayAsync { get; set; }
        public ViewContext ViewContext { get; set; }
        public object Value { get; set; }
    }
}