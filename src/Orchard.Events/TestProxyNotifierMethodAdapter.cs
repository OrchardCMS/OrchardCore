using Microsoft.Framework.Notification;
using Microsoft.Framework.Notification.Internal;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Orchard.Events {
    public class TestProxyNotifierMethodAdapter : INotifierMethodAdapter {
        public Func<object, object, bool> Adapt(MethodInfo method, Type inputType) {
            return CreateProxyMethod(method, inputType);
        }

        private static readonly ProxyTypeCache _cache = new ProxyTypeCache();

        public static Func<object, object, bool> CreateProxyMethod(MethodInfo method, Type inputType) {
            var name = string.Format("Proxy_Method_From_{0}_To_{1}", inputType.Name, method);

            // Define the method-adapter as Func<object, object, bool>, we'll do casts inside.
            var dynamicMethod = new DynamicMethod(
                name,
                returnType: typeof(bool),
                parameterTypes: new Type[] { typeof(object), typeof(object) },
                restrictedSkipVisibility: true);

            var parameters = method.GetParameters();
            var mappings = new PropertyInfo[parameters.Length];

            var inputTypeInfo = inputType.GetTypeInfo();
            for (var i = 0; i < parameters.Length; i++) {
                var property = inputTypeInfo.GetDeclaredProperty(parameters[i].Name);
                if (property == null) {
                    continue;
                }
                else {
                    mappings[i] = property;
                }
            }

            var il = dynamicMethod.GetILGenerator();

            // Define a local for each method parameters. This is needed when the parameter is
            // a value type, but we'll do it for all for simplicity.
            for (var i = 0; i < parameters.Length; i++) {
                il.DeclareLocal(parameters[i].ParameterType);
            }

            var endLabel = il.DefineLabel(); // Marks the 'return'
            var happyPathLabel = il.DefineLabel(); // Marks the 'happy path' - wherein we dispatch to the listener.

            //// Check if the input value is of the type we can handle.
            il.Emit(OpCodes.Ldarg_1); // The event-data
            il.Emit(OpCodes.Isinst, inputType);
            il.Emit(OpCodes.Brtrue, happyPathLabel);

            // We get here if the event-data doesn't match the type we can handle.
            //
            // Push 'false' onto the stack and return.
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Br, endLabel);

            // We get here if the event-data matches the type we can handle.
            il.MarkLabel(happyPathLabel);

            // Initialize locals to hold each parameter value.
            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];
                if (parameter.ParameterType.GetTypeInfo().IsValueType) {
                    // default-initialize each value type.
                    il.Emit(OpCodes.Ldloca_S, i);
                    il.Emit(OpCodes.Initobj, parameter.ParameterType);
                }
                else {
                    // null-initialize each reference type.
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stloc_S, i);
                }
            }

            // Evaluate all properties and store them in the locals.
            for (var i = 0; i < parameters.Length; i++) {
                var mapping = mappings[i];
                if (mapping != null) {
                    il.Emit(OpCodes.Ldarg_1); // The event-data
                    il.Emit(OpCodes.Castclass, inputType);

                    il.Emit(OpCodes.Callvirt, mapping.GetMethod);

                    // If the property-return-value requires a proxy, then create it.
                    if (!parameters[i].ParameterType.IsAssignableFrom(mapping.PropertyType)) {
                        var proxyType = ProxyTypeEmitter.GetProxyType(
                            _cache,
                            parameters[i].ParameterType,
                            mapping.PropertyType);
                        var proxyConstructor = proxyType.GetConstructors()[0];

                        il.Emit(OpCodes.Newobj, proxyConstructor);
                    }

                    il.Emit(OpCodes.Stloc_S, i);
                }
            }

            // Set up the call to the listener.
            //
            // Push the listener object, and then all of the argument values.
            il.Emit(OpCodes.Ldarg_0); // The listener
            il.Emit(OpCodes.Castclass, method.DeclaringType);

            // Push arguments onto the stack
            for (var i = 0; i < parameters.Length; i++) {
                il.Emit(OpCodes.Ldloc_S, i);
            }

            // Call the method in the listener
            il.Emit(OpCodes.Callvirt, method);

            // Success!
            //
            // Push 'true' onto the stack and return.
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Br, endLabel);

            // We expect that whoever branched to here put a boolean value (I4_0, I4_1) on top of the stack.
            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);

            return (Func<object, object, bool>)dynamicMethod.CreateDelegate(typeof(Func<object, object, bool>));
        }
    }
}
