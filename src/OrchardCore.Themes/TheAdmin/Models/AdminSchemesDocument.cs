using OrchardCore.Data.Documents;

namespace OrchardCore.Themes.TheAdmin.Models;

/// <summary>
/// The schemes of the admin. The theme seeds its default schemes once (Blue, Gray, Ocean...); admins edit, delete and
/// add schemes from there. Kept in one document behind Orchard Core's cached <c>IDocumentManager</c>: read on every
/// admin page, written only when a scheme is saved or deleted.
/// </summary>
public sealed class AdminSchemesDocument : Document
{
    /// <summary>
    /// Gets or sets the schemes, by id.
    /// </summary>
    public Dictionary<string, AdminScheme> Schemes { get; set; } = new(StringComparer.Ordinal);
}
