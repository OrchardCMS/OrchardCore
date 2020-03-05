using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Scripting.Providers
{
    public class LogProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _log;

        public LogProvider(ILogger<LogProvider> logger)
        {
            _log = new GlobalMethod
            {
                Name = "log",
                Method = serviceProvider => (Action<string, string, object>)((level, text, param) =>
                {
                    if (!Enum.TryParse<LogLevel>(level, true, out var logLevel))
                    {
                        logLevel = LogLevel.Information;
                    }
                    object[] args;
                    if (!(param is Array))
                    {
                        args = new[] { param };
                    }
                    else
                    {
                        args = (object[])param;
                    }
                    logger.Log(logLevel, text, args);
                })
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _log };
        }
    }
}
