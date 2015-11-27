using Microsoft.AspNet.Mvc.Rendering;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayContext
    {
        public DisplayHelper Display { get; set; }
        public ViewContext ViewContext { get; set; }
        public object Value { get; set; }
    }
}