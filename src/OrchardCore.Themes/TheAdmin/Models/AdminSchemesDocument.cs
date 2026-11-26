using OrchardCore.Data.Documents;

namespace OrchardCore.Themes.TheAdmin.Models;

/// <summary>
/// The schemes of TheAdmin. The theme's own schemes are imported once by a migration (see
/// <see cref="Migrations.AdminSchemesMigrations"/>); admins edit, delete and add schemes from there. Kept in one
/// document behind Orchard Core's cached <c>IDocumentManager</c>: read on every admin page, written only when a scheme
/// is saved or deleted.
/// </summary>
public sealed class AdminSchemesDocument : Document
{
    /// <summary>
    /// Gets or sets the schemes, by id.
    /// </summary>
    public Dictionary<string, AdminScheme> Schemes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the id of the site's default scheme. When empty, or when it names a scheme that is not there, the
    /// theme's default scheme is used (see <see cref="Schemes.TheAdminSchemes.DefaultId"/>).
    /// </summary>
    public string DefaultSchemeId { get; set; }
}
