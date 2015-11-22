using System;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.ShellBuilders
{
    /// <summary>
    /// The shell context represents the shell's state that is kept alive
    /// for the whole life of the application
    /// </summary>
    public class ShellContext : IDisposable
    {
        private bool _disposed = false;

        public ShellSettings Settings { get; set; }
        public ShellBlueprint Blueprint { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        public bool IsActived { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }

                Settings = null;
                Blueprint = null;

                // Disposes all the services registered for this shell
                (ServiceProvider as IDisposable).Dispose();

                _disposed = true;
            }
        }

        ~ShellContext()
        {
            Dispose(false);
        }
    }
}