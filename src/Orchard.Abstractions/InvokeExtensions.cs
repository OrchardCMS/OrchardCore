using System;
using System.Collections.Generic;
using Orchard.Security;
using Microsoft.Extensions.Logging;

namespace Orchard
{
    public static class InvokeExtensions
    {
        /// <summary>
        /// Safely invoke methods by catching non fatal exceptions and logging them
        /// </summary>
        public static void Invoke<TEvents>(this IEnumerable<TEvents> events, Action<TEvents> dispatch, ILogger logger)
        {
            foreach (var sink in events)
            {
                try
                {
                    dispatch(sink);
                }
                catch (Exception ex)
                {
                    if (IsLogged(ex))
                    {
                        logger.LogError(string.Format("{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name), ex);
                    }

                    if (ex.IsFatal())
                    {
                        throw;
                    }
                }
            }
        }

        public static IEnumerable<TResult> Invoke<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, TResult> dispatch, ILogger logger)
        {
            foreach (var sink in events)
            {
                TResult result = default(TResult);
                try
                {
                    result = dispatch(sink);
                }
                catch (Exception ex)
                {
                    if (IsLogged(ex))
                    {
                        logger.LogError(string.Format("{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name), ex);
                    }

                    if (ex.IsFatal())
                    {
                        throw;
                    }
                }

                yield return result;
            }
        }


        private static bool IsLogged(Exception ex)
        {
            return ex is OrchardSecurityException || !ex.IsFatal();
        }
    }
}