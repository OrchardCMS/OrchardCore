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
                    try
                    {
                        if (!Enum.TryParse<LogLevel>(level, true, out var logLevel))
                        {
                            logLevel = LogLevel.Information;
                        }
                        if (param == null)
                        {
#pragma warning disable CA2254 // Template should be a static expression
                            logger.Log(logLevel, text);
#pragma warning restore CA2254 // Template should be a static expression
                        }
                        else
                        {
                            object[] args;
                            if (param is not Array)
                            {
                                args = new[] { param };
                            }
                            else
                            {
                                args = (object[])param;
                            }

#pragma warning disable CA2254 // Template should be a static expression
                            logger.Log(logLevel, text, args);
#pragma warning restore CA2254 // Template should be a static expression
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, "Error logging text template {text} with param {param} from Scripting Engine.", text, param);
                    }
                }),
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _log };
        }
    }
}
