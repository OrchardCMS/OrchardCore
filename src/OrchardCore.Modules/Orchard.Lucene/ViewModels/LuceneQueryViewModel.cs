using System.ComponentModel.DataAnnotations;

namespace Orchard.Lucene.ViewModels
{
    public class LuceneQueryViewModel
    {
        public string[] Indices { get; set; }

        [Required]
        public string Index { get; set; }
        [Required]
        public string Query { get; set; }
        public bool ReturnContentItems { get; set; }
    }
}
