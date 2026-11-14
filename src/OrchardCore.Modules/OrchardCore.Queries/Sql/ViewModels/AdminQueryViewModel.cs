using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

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

    /// <summary>
    /// Gets or sets the editor of the SQL query runner, built by the display drivers of <see cref="AdminQueryViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
