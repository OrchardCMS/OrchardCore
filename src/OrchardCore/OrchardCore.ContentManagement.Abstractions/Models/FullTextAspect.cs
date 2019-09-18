using System.Text;

namespace OrchardCore.ContentManagement.Models
{
    public class FullTextAspect
    {
        public bool IsIndexed { get; set; } = true;
        public StringBuilder FullText { get; set; }
    }
}
