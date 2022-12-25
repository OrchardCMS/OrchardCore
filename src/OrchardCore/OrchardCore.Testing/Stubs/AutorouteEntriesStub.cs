using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Autoroute.Core.Services;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Testing.Stubs;

public class AutorouteEntriesStub : AutorouteEntries, IAutorouteEntriesStub
{
    public AutorouteEntriesStub() : base(null)
    {
    }

    public new void AddEntries(IEnumerable<AutorouteEntry> entries) => base.AddEntries(entries);

    public new void RemoveEntries(IEnumerable<AutorouteEntry> entries) => base.RemoveEntries(entries);

    protected override Task InitializeEntriesAsync() => Task.CompletedTask;
}
