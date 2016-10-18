using Microsoft.Extensions.Logging;

namespace Orchard.Environment.Shell.Builders
{
    internal class NonDisposableLoggerFactoryWrapper : ILoggerFactory
    {
        private readonly ILoggerFactory _wrapper;

        public NonDisposableLoggerFactoryWrapper(ILoggerFactory wrapper)
        {
            _wrapper = wrapper;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _wrapper.AddProvider(provider);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _wrapper.CreateLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}
