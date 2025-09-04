using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Queries.Sql.ViewModels;

public class AdminQueryViewModel
{
    public static string MatchAllQueryBase64 { get; } = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(@"SELECT * FROM ContentItemIndex"));

    public string DecodedQuery { get; set; }
    public string Parameters { get; set; } = "";

    [BindNever]
    public string RawSql { get; set; }

    [BindNever]
    public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

    [BindNever]
    public IEnumerable<dynamic> Documents { get; set; } = [];

    [BindNever]
    public string FactoryName { get; set; }
}
