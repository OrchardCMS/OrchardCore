using System;
using System.Collections.Generic;
using Orchard.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
            var results = new List<TResult>();

            foreach (var sink in events)
            {
                try
                {
                    var result = dispatch(sink);
                    results.Add(result);
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

            return results;
        }

        public static IEnumerable<TResult> Invoke<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, IEnumerable<TResult>> dispatch, ILogger logger)
        {
            var results = new List<TResult>();

            foreach (var sink in events)
            {
                try
                {
                    var result = dispatch(sink);
                    results.AddRange(result);
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

            return results;
        }


        /// <summary>
        /// Safely invoke methods by catching non fatal exceptions and logging them
        /// </summary>
        public static async Task InvokeAsync<TEvents>(this IEnumerable<TEvents> events, Func<TEvents, Task> dispatch, ILogger logger)
        {
            foreach (var sink in events)
            {
                try
                {
                    await dispatch(sink);
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

        public static async Task<IEnumerable<TResult>> InvokeAsync<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, Task<TResult>> dispatch, ILogger logger)
        {
            var results = new List<TResult>();

            foreach (var sink in events)
            {
                try
                {
                    var result = await dispatch(sink);
                    results.Add(result);
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

            return results;
        }

        public static async Task<IEnumerable<TResult>> InvokeAsync<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, Task<IEnumerable<TResult>>> dispatch, ILogger logger)
        {
            var results = new List<TResult>();

            foreach (var sink in events)
            {
                try
                {
                    var result = await dispatch(sink);
                    results.AddRange(result);
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

            return results;
        }

        private static bool IsLogged(Exception ex)
        {
            return ex is OrchardSecurityException || !ex.IsFatal();
        }
    }
}