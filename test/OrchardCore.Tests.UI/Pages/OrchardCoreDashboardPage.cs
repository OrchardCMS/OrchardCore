using Atata;

namespace OrchardCore.Tests.UI.Pages
{
    // Atata convention.
#pragma warning disable IDE0065 // Misplaced using directive
    using _ = OrchardCoreDashboardPage;
#pragma warning restore IDE0065 // Misplaced using directive

    [Url("Admin")]
    public sealed class OrchardCoreDashboardPage : OrchardCoreAdminPage<_>
    {
    }
}
