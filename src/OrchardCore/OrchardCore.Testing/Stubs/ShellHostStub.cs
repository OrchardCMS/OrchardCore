using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Events;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace OrchardCore.Testing.Stubs
{
    public class ShellHostStub : IShellHost
    {
        public ShellsEvent LoadingAsync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ShellEvent ReleasingAsync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ShellEvent ReloadingAsync { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IServiceCollection ServiceCollection { get; set; }

        public Action<Mock<IServiceProvider>> ConfigureScopeServiceProvider { get; set; }

        public ShellHostStub()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IShellHost>(this);
            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddScoped<IHttpContextAccessor>(x => new HttpContextAccessor());
            ServiceCollection = serviceCollection;
        }

        public Task ChangedAsync(ShellDescriptor descriptor, ShellSettings settings)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShellSettings> GetAllSettings()
        {
            throw new NotImplementedException();
        }

        public virtual Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task<ShellScope> GetScopeAsync(ShellSettings settings)
        {
            var serviceProvider = new Mock<IServiceProvider>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            var distributedLock = new Mock<IDistributedLock>();
            distributedLock.Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(new Mock<ILocker>().Object));

            serviceProvider
                .Setup(x => x.GetService(typeof(IDistributedLock)))
                .Returns(distributedLock.Object);

            var options = new Mock<IOptions<ShellContextOptions>>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IOptions<ShellContextOptions>)))
                .Returns(options.Object);

            if (ConfigureScopeServiceProvider != null)
            {
                ConfigureScopeServiceProvider(serviceProvider);
            }

            var scope = new ShellScope(new ShellContext() { ServiceProvider = serviceProvider.Object, Settings = settings, IsActivated = true });

            return Task.FromResult(scope);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<ShellContext> ListShellContexts()
        {
            throw new NotImplementedException();
        }

        public Task ReleaseShellContextAsync(ShellSettings settings, bool eventSource = true)
        {
            throw new NotImplementedException();
        }

        public Task ReloadShellContextAsync(ShellSettings settings, bool eventSource = true)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryGetSettings(string name, out ShellSettings settings)
        {
            settings = new ShellSettings(new ShellConfiguration(), new ShellConfiguration())
            {
                Name = name,
                RequestUrlHost = string.Empty,
                RequestUrlPrefix = string.Empty,
                State = TenantState.Running,
                VersionId = Guid.NewGuid().ToString("n")
            };
            return true;
        }

        public delegate ShellContext GetShellContextDelegate (string name);
        public GetShellContextDelegate GetShellContextImpl { get; set; }

        public virtual bool TryGetShellContext(string name, out ShellContext shellContext)
        {
            if (GetShellContextImpl != null)
            {
                try
                {
                    shellContext = GetShellContextImpl(name);
                    return true;
                }
                catch (Exception)
                {
                    shellContext = null;
                    return false;
                }
            }
            throw new NotImplementedException();

        }

        public Task UpdateShellSettingsAsync(ShellSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
