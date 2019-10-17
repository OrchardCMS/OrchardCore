namespace OrchardCore.Queries
{
    public class LuceneQueryResult: IQueryResult
    {
        public object Items { get; set; }
        public int Count { get; set; }
    }
}
