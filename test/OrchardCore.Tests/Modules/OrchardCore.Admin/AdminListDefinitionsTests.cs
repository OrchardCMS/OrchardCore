using System.Reflection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

/// <summary>
/// Checks the admin lists every module declares, e.g. <c>QueriesAdminList</c>, rather than one of them:
/// they are found by reflection, so a list added later is checked without touching this file.
/// </summary>
public class AdminListDefinitionsTests
{
    [Fact]
    public void EveryListHasItsOwnName()
    {
        var names = GetAdminLists()
            .Select(list => new { Type = list.Name, Name = (string)list.GetField("Name").GetRawConstantValue() })
            .ToList();

        Assert.NotEmpty(names);

        // Two lists sharing a name would share the columns a provider configures for it, and the
        // AdminList__{Name} alternate of one would render the rows of the other.
        var duplicates = names
            .GroupBy(list => list.Name)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(list => list.Type))})")
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void EveryColumnOfAListHasItsOwnNameAndRendersSomething()
    {
        var localizer = new NullStringLocalizer();

        foreach (var list in GetAdminLists())
        {
            var name = (string)list.GetField("Name").GetRawConstantValue();
            var columns = (List<AdminListColumn>)list
                .GetMethod("GetDefaultColumns", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, [localizer]);

            Assert.NotEmpty(columns);

            foreach (var column in columns)
            {
                Assert.False(string.IsNullOrWhiteSpace(column.Name), $"A column of the {name} list has no name.");

                // A column renders the zones it names, so one without any renders an empty cell.
                Assert.True(column.Zones is { Length: > 0 }, $"The {column.Name} column of the {name} list renders no zone.");
            }

            var columnNames = columns.Select(column => column.Name).ToList();

            // A provider finds a column by name, so two columns of a list cannot share one.
            Assert.Equal(columnNames.Count, columnNames.Distinct().Count());
        }
    }

    private static List<Type> GetAdminLists()
    {
        var lists = new List<Type>();

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
                // The lists are static classes named after the list they describe, e.g. QueriesAdminList.
                if (!type.IsAbstract || !type.IsSealed || !type.Name.EndsWith("AdminList", StringComparison.Ordinal))
                {
                    continue;
                }

                var nameField = type.GetField("Name", BindingFlags.Public | BindingFlags.Static);

                if (nameField is { IsLiteral: true } && nameField.FieldType == typeof(string) &&
                    type.GetMethod("GetDefaultColumns", BindingFlags.Public | BindingFlags.Static) != null)
                {
                    lists.Add(type);
                }
            }
        }

        return lists;
    }

    private sealed class NullStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
