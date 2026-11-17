using OrchardCore.Data.Migration;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Migrations;

/// <summary>
/// Seeds the theme's default schemes once, when the theme is first enabled (or on the first start of a site that had
/// it enabled before schemes existed). Admins edit and delete them from there; they are never seeded again, so a
/// deleted default stays deleted.
/// </summary>
public sealed class AdminSchemesMigrations : DataMigration
{
    private readonly AdminSchemeStore _store;

    public AdminSchemesMigrations(AdminSchemeStore store)
    {
        _store = store;
    }

    public async Task<int> CreateAsync()
    {
        await _store.SeedDefaultsAsync();

        return 1;
    }
}
