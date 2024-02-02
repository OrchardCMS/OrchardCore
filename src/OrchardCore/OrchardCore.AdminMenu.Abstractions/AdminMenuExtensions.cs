using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu;
public static class AdminMenuExtensions
{
    /// <summary>
    /// Registers an <see cref="AdminNode"/> implementation with JSON serialization.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    public static void AddAdminNode<T>(this IServiceCollection services) where T : AdminNode
    {
        services.AddJsonDerivedTypeInfo<T, AdminNode>();
        services.AddJsonDerivedTypeInfo<T, MenuItem>();
    }
    
}
