using System;
using System.Collections.Generic;
using Orchard.Localization;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly IDictionary<string, IList<IEventHandler>> _eventHandlers;
        private static readonly ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>> _delegateCache = new ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>>();

        public DefaultOrchardEventBus(IEnumerable<IEventHandler> eventHandlers) {
            IDictionary<string, IList<IEventHandler>> localEventHandlers
                = new Dictionary<string, IList<IEventHandler>>();

            foreach (var eventHandler in eventHandlers) {
                var interfaces = eventHandler.GetType().GetInterfaces();
                foreach (var interfaceType in interfaces) {

                    // register named instance for each interface, for efficient filtering inside event bus
                    // IEventHandler derived classes only
                    if (interfaceType.GetInterfaces().Any(x => x.Name == typeof(IEventHandler).Name)) {
                        if (!localEventHandlers.ContainsKey(interfaceType.Name)) {
                            localEventHandlers[interfaceType.Name] = new List<IEventHandler> { eventHandler };
                        }
                        else {
                            localEventHandlers[interfaceType.Name].Add(eventHandler);
                        }
                    }
                }
            }

            _eventHandlers = localEventHandlers;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
            // call ToArray to ensure evaluation has taken place
            return NotifyHandlers(messageName, eventData).ToArray();
        }

        private IEnumerable<object> NotifyHandlers(string messageName, IDictionary<string, object> eventData) {
            string[] parameters = messageName.Split('.');
            if (parameters.Length != 2) {
                throw new ArgumentException(T("{0} is not formatted correctly", messageName));
            }
            string interfaceName = parameters[0];
            string methodName = parameters[1];

            if (!_eventHandlers.ContainsKey(interfaceName)) {
                yield return Enumerable.Empty<object>();
            }
            else {
                var eventHandlers = _eventHandlers[interfaceName];
                foreach (var eventHandler in eventHandlers) {
                    IEnumerable returnValue;
                    if (TryNotifyHandler(eventHandler, messageName, interfaceName, methodName, eventData, out returnValue)) {
                        if (returnValue != null) {
                            foreach (var value in returnValue) {
                                yield return value;
                            }
                        }
                    }
                }
            }
        }

        private bool TryNotifyHandler(IEventHandler eventHandler, string messageName, string interfaceName, string methodName, IDictionary<string, object> eventData, out IEnumerable returnValue) {
            try {
                return TryInvoke(eventHandler, messageName, interfaceName, methodName, eventData, out returnValue);
            }
            catch {
                returnValue = null;
                return false;
            }
        }

        private static bool TryInvoke(IEventHandler eventHandler, string messageName, string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            var matchingInterface = eventHandler.GetType().GetInterfaces().Single(x => x.Name == interfaceName);
            return TryInvokeMethod(eventHandler, matchingInterface, messageName, interfaceName, methodName, arguments, out returnValue);
        }

        private static bool TryInvokeMethod(IEventHandler eventHandler, Type interfaceType, string messageName, string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            var key = eventHandler.GetType().FullName + "_" + messageName + "_" + String.Join("_", arguments.Keys);
            var cachedDelegate = _delegateCache.GetOrAdd(key, k => {
                var method = GetMatchingMethod(eventHandler, interfaceType, methodName, arguments);
                return method != null
                    ? Tuple.Create(method.GetParameters(), DelegateHelper.CreateDelegate<IEventHandler>(eventHandler.GetType(), method))
                    : null;
            });

            if (cachedDelegate != null) {
                var args = cachedDelegate.Item1.Select(methodParameter => arguments[methodParameter.Name]).ToArray();
                var result = cachedDelegate.Item2(eventHandler, args);

                returnValue = result as IEnumerable;
                if (result != null && (returnValue == null || result is string))
                    returnValue = new[] { result };
                return true;
            }

            returnValue = null;
            return false;
        }

        private static MethodInfo GetMatchingMethod(IEventHandler eventHandler, Type interfaceType, string methodName, IDictionary<string, object> arguments) {
            var allMethods = new List<MethodInfo>(interfaceType.GetMethods());
            var candidates = new List<MethodInfo>(allMethods);

            foreach (var method in allMethods) {
                if (string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase)) {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (var parameter in parameterInfos) {
                        if (!arguments.ContainsKey(parameter.Name)) {
                            candidates.Remove(method);
                            break;
                        }
                    }
                }
                else {
                    candidates.Remove(method);
                }
            }

            // treating common case separately
            if (candidates.Count == 1) {
                return candidates[0];
            }

            if (candidates.Count != 0) {
                return candidates.OrderBy(x => x.GetParameters().Length).Last();
            }

            return null;
        }
    }
}
