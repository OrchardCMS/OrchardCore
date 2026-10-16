
namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

/// <summary>
/// Checks list names published as constants for reuse.
/// </summary>
public class AdminListDefinitionsTests
{
    [Fact]
    public void PublishedListNames_AreUnique()
    {
        var names = GetAdminLists()
            .Select(list => new { Type = list.Name, Name = GetName(list) })
            .ToList();

        Assert.NotEmpty(names);

        // Two lists sharing a name would share the column providers targeting it, and the
        // AdminList__{Name} alternate of one would render the rows of the other.
        var duplicates = names
            .GroupBy(list => list.Name)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(list => list.Type))})")
            .ToList();

        Assert.Empty(duplicates);
    }

    private static string GetName(Type list)
        => (string)list.GetField("Name").GetRawConstantValue();

    private static List<Type> GetAdminLists()
        => GetExportedTypes()
            // Lists with reused names can publish them on a dedicated static class.
            .Where(type => type.IsAbstract && type.IsSealed && type.Name.EndsWith("AdminList", StringComparison.Ordinal))
            .Where(type => type.GetField("Name", BindingFlags.Public | BindingFlags.Static) is { IsLiteral: true } field && field.FieldType == typeof(string))
            .ToList();

    private static IEnumerable<Type> GetExportedTypes()
    {
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "OrchardCore.*.dll"))
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (BadImageFormatException)
            {
                continue;
            }

            foreach (var type in assembly.GetExportedTypes())
            {
                yield return type;
            }
        }
    }

}
