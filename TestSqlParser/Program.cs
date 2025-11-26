using OrchardCore.Queries.Sql;

var sqls = new[]
{
    "select a where a = @b",
    "select a where a = @b limit 10",
    "select a where a = @b limit @limit",
    "select a limit @limit",
};

foreach (var sql in sqls)
{
    Console.WriteLine($"\nTesting: {sql}");
    if (ParlotSqlParser.TryParse(sql, out var result, out var error))
    {
        Console.WriteLine("✓ Parse successful");
    }
    else
    {
        Console.WriteLine($"✗ Parse failed");
        if (error != null)
        {
            Console.WriteLine($"  Error: {error.Message}");
        }
    }
}
