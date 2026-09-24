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
    public async Task EveryList_HasColumnsThatRenderSomething()
    {
        var providers = GetOwnerProviders();

        Assert.NotEmpty(providers);

        foreach (var list in GetAdminLists())
        {
            var name = GetName(list);
            var context = new AdminListColumnsContext(name);

            // A provider adding a column without a name, or one the list already has, throws here.
            foreach (var provider in providers)
            {
                await provider.BuildAsync(context, TestContext.Current.CancellationToken);
            }

            // A list without columns can only be rendered with the List layout.
            Assert.True(context.Columns.Count > 0, $"No provider declares the columns of the {name} list.");

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

    // The providers of the modules owning the lists only need a localizer for the headers of their columns.
    private static List<IAdminListColumnProvider> GetOwnerProviders()
    {
        var providers = new List<IAdminListColumnProvider>();

        foreach (var type in GetExportedTypes().Where(type => !type.IsAbstract && typeof(IAdminListColumnProvider).IsAssignableFrom(type)))
        {
            var localizerType = typeof(IStringLocalizer<>).MakeGenericType(type);
            var constructor = type.GetConstructor([localizerType]);

            if (constructor is null)
            {
                continue;
            }

            var localizer = Activator.CreateInstance(typeof(NullStringLocalizer<>).MakeGenericType(type));

            providers.Add((IAdminListColumnProvider)constructor.Invoke([localizer]));
        }

        return providers;
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
