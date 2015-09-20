using System;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.ShellBuilders {
    public class ShellContext : IDisposable {
        private bool _disposed = false;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public IServiceProvider LifetimeScope { get; set; }
        public IOrchardShell Shell { get; set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {

                if (disposing) {
                    
                }

                Settings = null;
                Blueprint = null;
                LifetimeScope = null;
                Shell = null;

                _disposed = true;
            }
        }

        ~ShellContext() {
            Dispose(false);
        }
    }
}