using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Tenants.ViewModels;

public class SetupApiViewModel
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string SiteName { get; set; }

    public string DatabaseProvider { get; set; }

    public string ConnectionString { get; set; }

    public string TablePrefix { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; }

    /// <summary>
    /// The name of a recipe available in the app.
    /// </summary>
    public string RecipeName { get; set; }

    /// <summary>
    /// A JSON string representing a custom recipe.
    /// </summary>
    public string Recipe { get; set; }

    public string SiteTimeZone { get; set; }

    public string Schema { get; set; }
}
