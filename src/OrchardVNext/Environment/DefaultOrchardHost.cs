using OrchardVNext.Environment.Extensions;
using System;

namespace OrchardVNext.Environment {
    public interface IOrchardHost {
        void Initialize();
    }

    public class DefaultOrchardHost : IOrchardHost {
        //private readonly IExtensionLoaderCoordinator _extensionLoaderCoordinator;
        //public DefaultOrchardHost(IExtensionLoaderCoordinator extensionLoaderCoordinator) {
        //    _extensionLoaderCoordinator = extensionLoaderCoordinator;
        //}
        void IOrchardHost.Initialize() {
            Logger.Information("Initialize Host");

            //_extensionLoaderCoordinator.SetupExtensions();

            Logger.Information("Host Initialized");
        }
    }
}