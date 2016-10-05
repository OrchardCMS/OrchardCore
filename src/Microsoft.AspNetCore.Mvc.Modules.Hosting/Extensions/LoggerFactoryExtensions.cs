using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Hosting.Extensions
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddOrchardLogging(
            this ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
        {

            return loggerFactory;
        }
    }
}