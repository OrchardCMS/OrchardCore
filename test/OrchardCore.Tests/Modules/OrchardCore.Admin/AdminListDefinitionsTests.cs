using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

/// <summary>
/// Checks the admin lists every module declares, e.g. <c>QueriesAdminList</c>, and the drivers declaring their
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

        // Two lists sharing a name would share the columns a driver declares for it, and the
        // AdminList__{Name} alternate of one would render the rows of the other.
        var duplicates = names
            .GroupBy(list => list.Name)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(list => list.Type))})")
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public async Task EveryList_HasADriverDeclaringColumnsThatRenderSomething()
    {
        var lists = GetAdminLists();

        Assert.NotEmpty(lists);

        foreach (var list in lists)
        {
            var name = GetName(list);

            // The driver of a list is named after it, e.g. QueriesAdminListColumnDriver, and registered for its items.
            var (driver, model) = CreateOwnerDriver(list);

            Assert.True(driver != null, $"The {name} list has no {list.Name}ColumnDriver.");

            // A driver adding a column without a name, or one the list already has, throws here.
            var context = new AdminListColumnsContext(name, null, new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape())), null, null, null);
            var buildColumns = typeof(IAdminListColumnDriver<>).MakeGenericType(model).GetMethod(nameof(IAdminListColumnDriver<object>.BuildColumnsAsync));
            await (Task<IDisplayResult>)buildColumns.Invoke(driver, [context]);

            // A list without columns can only be rendered with the List layout.
            Assert.True(context.Columns.Count > 0, $"{list.Name}ColumnDriver declares no column.");

            // A column renders the zones it names, then the cells the drivers build, so a column of a driver that
            // builds no cell renders an empty cell unless it names a zone.
            if (!BuildsCells(driver))
            {
                foreach (var column in context.Columns)
                {
                    Assert.True(column.Zones is { Length: > 0 }, $"The {column.Name} column of the {name} list renders no zone.");
                }
            }
        }
    }

    private static string GetName(Type list)
        => (string)list.GetField("Name").GetRawConstantValue();

    // Whether the driver overrides one of the methods of AdminListColumnDriver<TModel> building the cells of a row.
    private static bool BuildsCells(object driver)
    {
        return driver.GetType().GetMethods()
            .Where(method => method.Name is nameof(AdminListColumnDriver<object>.Cells) or nameof(AdminListColumnDriver<object>.CellsAsync))
            .Any(method => !method.DeclaringType.IsGenericType);
    }

    private static List<Type> GetAdminLists()
        => GetExportedTypes()
            // The lists are static classes named after the list they describe, e.g. QueriesAdminList.
            .Where(type => type.IsAbstract && type.IsSealed && type.Name.EndsWith("AdminList", StringComparison.Ordinal))
            .Where(type => type.GetField("Name", BindingFlags.Public | BindingFlags.Static) is { IsLiteral: true } field && field.FieldType == typeof(string))
            .ToList();

    // The driver of the module owning a list only needs a localizer for the headers of its columns.
    private static (object Driver, Type Model) CreateOwnerDriver(Type list)
    {
        var type = list.Assembly.GetType($"{list.Namespace}.{list.Name}ColumnDriver");

        var model = type?.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAdminListColumnDriver<>))
            ?.GetGenericArguments()[0];

        if (model is null)
        {
            return (null, null);
        }

        var constructor = type.GetConstructor([typeof(IStringLocalizer<>).MakeGenericType(type)]);
        var localizer = Activator.CreateInstance(typeof(NullStringLocalizer<>).MakeGenericType(type));

        return (constructor?.Invoke([localizer]), model);
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
