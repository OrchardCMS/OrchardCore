using System.Collections.Generic;
using Orchard.DependencyInjection;

namespace Orchard.Hosting.Middleware
{
    /// <summary>
    /// Represents a provider that makes Owin middlewares available for the Orchard Owin pipeline.
    /// </summary>
    /// <remarks>
    /// If you want to write an Owin middleware and inject it into the Orchard Owin pipeline, implement this interface. For more information
    /// about Owin <see cref="!:http://owin.org/">http://owin.org/</see>.
    /// </remarks>
    public interface IMiddlewareProvider : ISingletonDependency
    {
        /// <summary>
        /// Gets <see cref="MiddlewareRegistration"/> objects that will be used to alter the Orchard Owin pipeline.
        /// </summary>
        /// <returns><see cref="MiddlewareRegistration"/> objects that will be used to alter the Orchard Owin pipeline.</returns>
        IEnumerable<MiddlewareRegistration> GetMiddlewares();
    }
}