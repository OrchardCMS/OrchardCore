using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.FileSystems.AppData;
using OrchardVNext.FileSystems.VirtualPath;
using OrchardVNext.FileSystems.WebSite;
using OrchardVNext.Routing;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace OrchardVNext.Environment {
    public class OrchardStarter {
        private static void CreateHostContainer(IApplicationBuilder app) {
            app.UseServices(services => {
                services.AddSingleton<IHostEnvironment, DefaultHostEnvironment>();
                services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();

                services.AddSingleton<IInlineConstraintResolver, DefaultInlineConstraintResolver>();

                services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
                services.AddSingleton<IAppDataFolder, AppDataFolder>();
                services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();
                
                services.AddTransient<IOrchardHost, DefaultOrchardHost>();
                {
                    services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                    services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                    {
                        services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                        {
                            services.AddSingleton<IExtensionManager, ExtensionManager>();
                            {
                                services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();
                                services.AddSingleton<IExtensionFolders, ModuleFolders>();
                                services.AddSingleton<IExtensionFolders, CoreModuleFolders>();
                                services.AddSingleton<IExtensionFolders, ThemeFolders>();

                                services.AddSingleton<IExtensionLoader, DefaultExtensionLoader>();
                            }
                        }

                        services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                    }
                };

                services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();

            });

            app.UseMiddleware<OrchardContainerMiddleware>();
            app.UseMiddleware<OrchardShellHostMiddleware>();
            app.UseMiddleware<OrchardRouterMiddleware>();
        }

        public static IOrchardHost CreateHost(IApplicationBuilder app) {
            CreateHostContainer(app);

            return app.ApplicationServices.GetService<IOrchardHost>();
        }
    }

    public class TypeActivator : ITypeActivator {
        public object CreateInstance(IServiceProvider services, Type instanceType, params object[] parameters) {
            int bestLength = -1;
            ConstructorMatcher bestMatcher = null;

            foreach (var matcher in instanceType
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(c => !c.IsStatic)
                .Select(constructor => new ConstructorMatcher(constructor))) {
                var length = matcher.Match(parameters);
                if (length == -1) {
                    continue;
                }
                if (bestLength < length) {
                    bestLength = length;
                    bestMatcher = matcher;
                }
            }

            if (bestMatcher == null) {
                throw new InvalidOperationException(instanceType.ToString());
            }

            return bestMatcher.CreateInstance(services);
        }

        class ConstructorMatcher {
            private readonly ConstructorInfo _constructor;
            private readonly ParameterInfo[] _parameters;
            private readonly object[] _parameterValues;
            private readonly bool[] _parameterValuesSet;

            public ConstructorMatcher(ConstructorInfo constructor) {
                _constructor = constructor;
                _parameters = _constructor.GetParameters();
                _parameterValuesSet = new bool[_parameters.Length];
                _parameterValues = new object[_parameters.Length];
            }

            public int Match(object[] givenParameters) {

                var applyIndexStart = 0;
                var applyExactLength = 0;
                for (var givenIndex = 0; givenIndex != givenParameters.Length; ++givenIndex) {
                    var givenType = givenParameters[givenIndex] == null ? null : givenParameters[givenIndex].GetType().GetTypeInfo();
                    var givenMatched = false;

                    for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != _parameters.Length; ++applyIndex) {
                        if (_parameterValuesSet[applyIndex] == false &&
                            _parameters[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType)) {
                            givenMatched = true;
                            _parameterValuesSet[applyIndex] = true;
                            _parameterValues[applyIndex] = givenParameters[givenIndex];
                            if (applyIndexStart == applyIndex) {
                                applyIndexStart++;
                                if (applyIndex == givenIndex) {
                                    applyExactLength = applyIndex;
                                }
                            }
                        }
                    }

                    if (givenMatched == false) {
                        return -1;
                    }
                }
                return applyExactLength;
            }
            private readonly ConcurrentDictionary<Type, Func<ServiceProvider, object>> _realizedServices = new ConcurrentDictionary<Type, Func<ServiceProvider, object>>();

            public object CreateInstance(IServiceProvider _services) {
                for (var index = 0; index != _parameters.Length; ++index) {
                    if (_parameterValuesSet[index] == false) {
                        var value = _services.GetService(_parameters[index].ParameterType);
                        if (value == null) {
                            if (!_parameters[index].HasDefaultValue) {
                                throw new InvalidOperationException(string.Format("{0}, {1}",
                                    _constructor.DeclaringType,
                                    _parameters[index].ParameterType));
                            }
                            else {
                                _parameterValues[index] = _parameters[index].DefaultValue;
                            }
                        }
                        else {
                            _parameterValues[index] = value;
                        }
                    }
                }
                return _constructor.Invoke(_parameterValues);
            }
        }
    }
}
