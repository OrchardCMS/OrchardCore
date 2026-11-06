namespace OrchardCore.Localization.Tests;

public class SharedStringLocalizerTests
{
    static SharedStringLocalizerTests()
        => SharedStringLocalizer.Initialize(new DummyStringLocalizerFactory());

    [Fact]
    public void DeferredLocalizedString_Value()
    {
        var deferredLocalizedString = LocalizationPermissions.ManageCultures.Description;

        Assert.Equal("Gérer la culture prise en charge", deferredLocalizedString.Value);
    }

    [Fact]
    public void DeferredLocalizedString_ImplicitCastingToString()
    {
        var deferredLocalizedString = LocalizationPermissions.ManageCultures.Description;

        string description = deferredLocalizedString;

        Assert.Equal("Gérer la culture prise en charge", description);
    }

    [Fact]
    public void DeferredLocalizedString_ImplicitCastingToLocalizedString()
    {
        var deferredLocalizedString = LocalizationPermissions.ManageCultures.Description;

        LocalizedString localizationString = deferredLocalizedString;

        Assert.Equal("Manage supported culture", localizationString.Name);
        Assert.Equal("Gérer la culture prise en charge", localizationString.Value);
        Assert.False(localizationString.ResourceNotFound);
    }

    [Fact]
    public void DeferredLocalizedString_NotFound()
    {
        var deferredLocalizedString = AdminPermissions.Unknown.Description;

        Assert.Equal("Unknown", deferredLocalizedString.Value);
        Assert.True(deferredLocalizedString.Value.ResourceNotFound);
    }

    #region Localizer
    private sealed class DummyStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ConcurrentDictionary<Type, IStringLocalizer> localizers = new();

        public IStringLocalizer Create(Type resourceSource)
            => localizers.GetOrAdd(resourceSource, t => new DummyStringLocalizer(t));

        public IStringLocalizer Create(string baseName, string location)
            => throw new NotSupportedException("Not used in tests.");
    }

    private sealed class DummyStringLocalizer : IStringLocalizer
    {
        private static readonly Dictionary<Type, List<LocalizedString>> _localizationResources = new Dictionary<Type, List<LocalizedString>>
        {
            {
                typeof(AdminPermissions),
                new List<LocalizedString>
                {
                    new LocalizedString("Access admin panel", "Accéder au panneau d'administration"),
                    new LocalizedString("Manage Admin Settings", "Gérer les paramètres d'administration"),
                }
            },
            {
                typeof(LocalizationPermissions),
                new List<LocalizedString>
                {
                    new LocalizedString("Manage supported culture", "Gérer la culture prise en charge"),
                }
            },
        };

        private readonly Type _resource;

        public DummyStringLocalizer(Type resource) => _resource = resource;

        public LocalizedString this[string name]
        {
            get
            {
                if (_localizationResources.TryGetValue(_resource, out var localizationStrings))
                {
                    var localizedString = localizationStrings.FirstOrDefault(ls => ls.Name == name);
                    if (localizedString != null)
                    {
                        return localizedString;
                    }
                }

                return new LocalizedString(name, name, resourceNotFound: true);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (_localizationResources.TryGetValue(_resource, out var localizationStrings))
                {
                    var localizedString = localizationStrings.FirstOrDefault(ls => ls.Name == name);
                    if (localizedString != null)
                    {
                        return new LocalizedString(name, string.Format(localizedString.Value, arguments), resourceNotFound: false);
                    }
                }

                return new LocalizedString(name, string.Format(name, arguments), resourceNotFound: true);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => _localizationResources.TryGetValue(_resource, out var localizationStrings)
                ? localizationStrings
                : Array.Empty<LocalizedString>();
    }

    #endregion

    #region Test Permissions
    private sealed class MyPermission(string name, DeferredLocalizedString description)
    {
        public string Name { get; set; } = name;

        public DeferredLocalizedString Description { get; set; } = description;
    }

    private static class AdminPermissions
    {
        public static readonly MyPermission AccessAdminPanel = new("AccessAdminPanel", SharedStringLocalizer.Get(typeof(AdminPermissions), "Access admin panel"));
        public static readonly MyPermission ManageAdminSettings = new("ManageAdminSettings", SharedStringLocalizer.Get(typeof(AdminPermissions), "Manage admin settings"));
        public static readonly MyPermission Unknown = new("Unknown", SharedStringLocalizer.Get(typeof(AdminPermissions), "Unknown"));
    }

    private static class LocalizationPermissions
    {
        public static readonly MyPermission ManageCultures = new("ManageCultures", SharedStringLocalizer.Get(typeof(LocalizationPermissions), "Manage supported culture"));
    }
#endregion
}
