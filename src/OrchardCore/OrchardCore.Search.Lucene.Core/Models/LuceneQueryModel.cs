namespace OrchardCore.Search.Lucene.Models;

public class LuceneQueryModel
{
    public string IndexName { set; get; }
    public string Query { set; get; }
    public string Parameters { set; get; }
}
