using System.Diagnostics.CodeAnalysis;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Environment.Shell;

public static class ShellContextExtensions
{
    /// <summary>
    /// Whether or not the tenant is only a placeholder built on loading, releasing or reloading.
    /// </summary>
    public static bool IsPlaceholder([NotNullWhen(true)] this ShellContext? context) => context is ShellContext.PlaceHolder;

    /// <summary>
    /// Whether or not the tenant is only the placeholder pre-created on first loading.
    /// </summary>
    public static bool IsPreCreated([NotNullWhen(true)] this ShellContext? context) => context is ShellContext.PlaceHolder { PreCreated: true };

    /// <summary>
    /// Whether or not the tenant container has been built on a first demand.
    /// </summary>
    public static bool HasServices([NotNullWhen(true)] this ShellContext? context) => context is { ServiceProvider: not null };

    /// <summary>
    /// Whether or not the tenant pipeline has been built on a first request.
    /// </summary>
    public static bool HasPipeline([NotNullWhen(true)] this ShellContext? context) => context is { Pipeline: not null };

    /// <summary>
    /// Whether or not the tenant is in use in at least one active scope.
    /// </summary>
    public static bool IsActive([NotNullWhen(true)] this ShellContext? context) => context is { ActiveScopes: > 0 };

    /// <summary>
    /// Marks this instance as using unshared <see cref="ShellContext.Settings"/> that can be disposed.
    /// </summary>
    /// <remarks>
    /// This is the default but can be used if <see cref="WithSharedSettings"/> may have been called.
    /// </remarks>
    public static ShellContext WithoutSharedSettings(this ShellContext context)
    {
        context.SharedSettings = false;
        return context;
    }

    /// <summary>
    /// Marks this instance as using shared <see cref="ShellContext.Settings"/> that should not be disposed.
    /// </summary>
    public static ShellContext WithSharedSettings(this ShellContext context)
    {
        context.SharedSettings = true;
        return context;
    }
}
