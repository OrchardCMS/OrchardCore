using System;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell
{
    public interface IShellScopePreDisposable : IDisposable
    {
    }

    public class ShellScopePreDisposable : IShellScopePreDisposable
    {
        private readonly IDisposable _disposable;

        public ShellScopePreDisposable(IDisposable disposable)
        {
            _disposable = disposable;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }

    public static class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection AddShellScopePreDisposable<TService>(this IServiceCollection services) where TService : class
        {
            return services.AddScoped<IShellScopePreDisposable>(sp => new ShellScopePreDisposable(sp.GetService(typeof(TService)) as IDisposable));
        }
    }
}