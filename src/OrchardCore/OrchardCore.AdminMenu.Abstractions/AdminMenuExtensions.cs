using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.Models;
using OrchardCore.AdminMenu.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu;
public static class AdminMenuExtensions
{
    public static IServiceCollection AddAdminNode<TNode, TNodeBuilder, TNodeDriver>(this IServiceCollection services)
        where TNode : AdminNode, new()
        where TNodeBuilder : class, IAdminNodeNavigationBuilder
        where TNodeDriver : class, IDisplayDriver<MenuItem>
    {
        services.AddAdminNode<TNode, TNodeBuilder>();

        services.AddDisplayDriver<MenuItem, TNodeDriver>();

        return services;
    }

    public static IServiceCollection AddAdminNode<TNode, TNodeBuilder>(this IServiceCollection services)
        where TNode : AdminNode, new()
        where TNodeBuilder : class, IAdminNodeNavigationBuilder
    {
        services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<TNode>());
        services.AddScoped<IAdminNodeNavigationBuilder, TNodeBuilder>();

        services.AddAdminNodeDerivedType<TNode>();

        return services;
    }

    /// <summary>
    /// Registers an <see cref="AdminNode"/> implementation with JSON serialization.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddAdminNodeDerivedType<T>(this IServiceCollection services) where T : AdminNode
    {
        services.AddJsonDerivedTypeInfo<T, AdminNode>();
        services.AddJsonDerivedTypeInfo<T, MenuItem>();

        return services;
    }
}
