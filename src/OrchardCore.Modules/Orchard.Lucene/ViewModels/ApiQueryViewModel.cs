namespace Orchard.Lucene.ViewModels
{
    public class ApiQueryViewModel
    {
        public string IndexName { get; set; }
        public string Query { get; set; }
        public bool ContentItems { get; set; }
        public string Parameters { get; set; }
    }
}
