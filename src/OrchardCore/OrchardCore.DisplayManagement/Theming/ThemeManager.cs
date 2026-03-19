using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Theming;

public class ThemeManager : IThemeManager
{
    private readonly IEnumerable<IThemeSelector> _themeSelectors;
    private readonly IExtensionManager _extensionManager;
    private readonly object _syncLock = new();

    private Task<IExtensionInfo> _theme;

    public ThemeManager(
        IEnumerable<IThemeSelector> themeSelectors,
        IExtensionManager extensionManager)
    {
        _themeSelectors = themeSelectors;
        _extensionManager = extensionManager;
    }

    public Task<IExtensionInfo> GetThemeAsync()
    {
        // For performance reason, processes the current theme only once per scope (request).
        // This can't be cached as each request gets a different value.
        if (_theme != null)
        {
            return _theme;
        }

        lock (_syncLock)
        {
            return _theme ??= CreateThemeTask();
        }
    }

    private Task<IExtensionInfo> CreateThemeTask()
    {
        var completionSource = new TaskCompletionSource<IExtensionInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = GetThemeAwaitedAsync(completionSource);

        return completionSource.Task;

        async Task GetThemeAwaitedAsync(TaskCompletionSource<IExtensionInfo> completionSource)
        {
            try
            {
                var theme = await GetThemeInternalAsync();

                if (theme == null)
                {
                    lock (_syncLock)
                    {
                        if (_theme == completionSource.Task)
                        {
                            _theme = null;
                        }
                    }
                }

                completionSource.SetResult(theme);
            }
            catch (Exception exception)
            {
                lock (_syncLock)
                {
                    if (_theme == completionSource.Task)
                    {
                        _theme = null;
                    }
                }

                completionSource.SetException(exception);
            }
        }
    }

    private async Task<IExtensionInfo> GetThemeInternalAsync()
    {
        var themeResults = new List<ThemeSelectorResult>();
        foreach (var themeSelector in _themeSelectors)
        {
            var themeResult = await themeSelector.GetThemeAsync();
            if (themeResult != null)
            {
                themeResults.Add(themeResult);
            }
        }

        if (themeResults.Count == 0)
        {
            return null;
        }

        themeResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));

        // Try to load the theme to ensure it's present
        foreach (var theme in themeResults)
        {
            var t = _extensionManager.GetExtension(theme.ThemeName);

            if (t.Exists)
            {
                return new ThemeExtensionInfo(t);
            }
        }

        // No valid theme. Don't save the result right now.
        return null;
    }
}
