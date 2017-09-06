namespace OrchardCore.Queries.Sql
{
    public class SqlQuery : Query
    {
        public SqlQuery() : base("Sql")
        {
        }

        public string Template { get; set; }
        public bool ReturnDocuments { get; set; }
    }
}
