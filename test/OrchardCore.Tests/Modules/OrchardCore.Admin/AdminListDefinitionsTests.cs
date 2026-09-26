using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

/// <summary>
/// Checks the admin lists every module declares, e.g. <c>QueriesAdminList</c>, and the providers declaring their
/// columns, rather than one of them: they are found by reflection, so a list added later is checked without
/// touching this file.
/// </summary>
public class AdminListDefinitionsTests
{
    [Fact]
    public void EveryList_HasItsOwnName()
    {
        var names = GetAdminLists()
            .Select(list => new { Type = list.Name, Name = GetName(list) })
            .ToList();

        Assert.NotEmpty(names);

        // Two lists sharing a name would share the columns a provider declares for it, and the
        // AdminList__{Name} alternate of one would render the rows of the other.
        var duplicates = names
            .GroupBy(list => list.Name)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(list => list.Type))})")
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task EveryList_HasAProviderDeclaringColumnsThatRenderSomething()
    {
        var lists = GetAdminLists();

        Assert.NotEmpty(lists);

        foreach (var list in lists)
        {
            var name = GetName(list);

            // The provider of a list is named after it, e.g. QueriesAdminListColumnProvider, and registered for it.
            var provider = CreateOwnerProvider(list);

            Assert.True(provider != null, $"The {name} list has no {list.Name}ColumnProvider.");

            // A provider adding a column without a name, or one the list already has, throws here.
            var context = new AdminListColumnsContext(name);
            await provider.BuildAsync(context, TestContext.Current.CancellationToken);

            // A list without columns can only be rendered with the List layout.
            Assert.True(context.Columns.Count > 0, $"{list.Name}ColumnProvider declares no column.");

            foreach (var column in context.Columns)
            {
                // A column renders the zones it names, so one without any renders an empty cell.
                Assert.True(column.Zones is { Length: > 0 }, $"The {column.Name} column of the {name} list renders no zone.");
            }
        }
    }

    private static string GetName(Type list)
        => (string)list.GetField("Name").GetRawConstantValue();

    private static List<Type> GetAdminLists()
        => GetExportedTypes()
            // The lists are static classes named after the list they describe, e.g. QueriesAdminList.
            .Where(type => type.IsAbstract && type.IsSealed && type.Name.EndsWith("AdminList", StringComparison.Ordinal))
            .Where(type => type.GetField("Name", BindingFlags.Public | BindingFlags.Static) is { IsLiteral: true } field && field.FieldType == typeof(string))
            .ToList();

    // The provider of the module owning a list only needs a localizer for the headers of its columns.
    private static IAdminListColumnProvider CreateOwnerProvider(Type list)
    {
        var type = list.Assembly.GetType($"{list.Namespace}.{list.Name}ColumnProvider");

        if (type is null || !typeof(IAdminListColumnProvider).IsAssignableFrom(type))
        {
            return null;
        }

        var constructor = type.GetConstructor([typeof(IStringLocalizer<>).MakeGenericType(type)]);
        var localizer = Activator.CreateInstance(typeof(NullStringLocalizer<>).MakeGenericType(type));

        return (IAdminListColumnProvider)constructor?.Invoke([localizer]);
    }

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

    public sealed class NullStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, name);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
