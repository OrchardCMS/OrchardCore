using System.Collections.Generic;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Testing.Stubs;

public interface IAutorouteEntriesStub : IAutorouteEntries
{
    void AddEntries(IEnumerable<AutorouteEntry> entries);

    void RemoveEntries(IEnumerable<AutorouteEntry> entries);
}
