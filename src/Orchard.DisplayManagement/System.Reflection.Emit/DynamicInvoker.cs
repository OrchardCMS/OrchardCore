using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Orchard.DisplayManagement.System.Reflection.Emit
{
    /// <summary>Delegate for calling a method that is not known at runtime.</summary>
    /// <param name="target">the object to be called or null if the call is to a static method.</param>
    /// <param name="parameters">the parameters to the method.</param>
    /// <returns>the return value for the method or null if it doesn't return anything.</returns>
    public delegate object FastInvokeHandler(object target, object[] parameters);

    /// <summary>Delegate for creating and object at runtime using the default constructor.</summary>
    /// <returns>the newly created object.</returns>
    public delegate object FastCreateInstanceHandler();

    /// <summary>Delegate to get an arbitraty property at runtime.</summary>
    /// <param name="target">the object instance whose property will be obtained.</param>
    /// <returns>the property value.</returns>
    public delegate object FastPropertyGetHandler(object target);

    /// <summary>Delegate to set an arbitrary property at runtime.</summary>
    /// <param name="target">the object instance whose property will be modified.</param>
    /// <param name="parameter"></param>
    public delegate void FastPropertySetHandler(object target, object parameter);

    /// <summary>Class with helper methods for dynamic invocation generating IL on the fly.</summary>
    public static class DynamicCalls
    {
        public static void EvaluateMethod(string testName, Action method, long times)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (long i = 0; i < times; i++)
            {
                method();
            }

            watch.Stop();

            Console.WriteLine(testName + ": " + watch.ElapsedMilliseconds + "ms");
        }
        public static FastInvokeHandler DynamicMethod(this MethodInfo methodInfo)
        {
            // generates a dynamic method to generate a FastInvokeHandler delegate
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            ParameterInfo[] parameters = methodInfo.GetParameters();

            Type[] paramTypes = new Type[parameters.Length];

            // copies the parameter types to an array
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                    paramTypes[i] = parameters[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = parameters[i].ParameterType;
            }

            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            // generates a local variable for each parameter
            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = ilGenerator.DeclareLocal(paramTypes[i], true);
            }

            // creates code to copy the parameters to the local variables
            for (int i = 0; i < paramTypes.Length; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg_1);
                EmitFastInt(ilGenerator, i);
                ilGenerator.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(ilGenerator, paramTypes[i]);
                ilGenerator.Emit(OpCodes.Stloc, locals[i]);
            }

            if (!methodInfo.IsStatic)
            {
                // loads the object into the stack
                ilGenerator.Emit(OpCodes.Ldarg_0);
            }

            // loads the parameters copied to the local variables into the stack
            for (int i = 0; i < paramTypes.Length; i++)
            {
                ilGenerator.Emit(parameters[i].ParameterType.IsByRef ? OpCodes.Ldloca_S : OpCodes.Ldloc, locals[i]);
            }

            // calls the method
            ilGenerator.EmitCall(!methodInfo.IsStatic ? OpCodes.Callvirt : OpCodes.Call, methodInfo, null);

            // creates code for handling the return value
            if (methodInfo.ReturnType == typeof(void))
            {
                ilGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                EmitBoxIfNeeded(ilGenerator, methodInfo.ReturnType);
            }

            // iterates through the parameters updating the parameters passed by ref
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(ilGenerator, i);
                    ilGenerator.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        ilGenerator.Emit(OpCodes.Box, locals[i].LocalType);
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                }
            }

            // returns the value to the caller
            ilGenerator.Emit(OpCodes.Ret);

            // converts the DynamicMethod to a FastInvokeHandler delegate to call to the method
            var invoker = (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
            return invoker;

        }

        /// <summary>Gets the instance creator delegate that can be use to create instances of the specified type.</summary>
        /// <param name="type">The type of the objects we want to create.</param>
        /// <returns>A delegate that can be used to create the objects.</returns>
        public static FastCreateInstanceHandler GetInstanceCreator(Type type)
        {
            // generates a dynamic method to generate a FastCreateInstanceHandler delegate
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, type, new Type[0], typeof(DynamicCalls));

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            // generates code to create a new object of the specified type using the default constructor
            ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));

            // returns the value to the caller
            ilGenerator.Emit(OpCodes.Ret);

            // converts the DynamicMethod to a FastCreateInstanceHandler delegate to create the object
            var creator = (FastCreateInstanceHandler)dynamicMethod.CreateDelegate(typeof(FastCreateInstanceHandler));
            creator.Invoke();
            return creator;
        }

        public static object ActiveInstance(Type type)
        {
            return GetInstanceCreator(type).Invoke();
        }
        public static FastPropertyGetHandler GetPropertyGetter(PropertyInfo propInfo)
        {
            // generates a dynamic method to generate a FastPropertyGetHandler delegate
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object) }, propInfo.DeclaringType);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            // loads the object into the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // calls the getter
            ilGenerator.EmitCall(OpCodes.Callvirt, propInfo.GetGetMethod(), null);

            // creates code for handling the return value
            EmitBoxIfNeeded(ilGenerator, propInfo.PropertyType);

            // returns the value to the caller
            ilGenerator.Emit(OpCodes.Ret);

            // converts the DynamicMethod to a FastPropertyGetHandler delegate to get the property
            FastPropertyGetHandler getter = (FastPropertyGetHandler)dynamicMethod.CreateDelegate(typeof(FastPropertyGetHandler));

            return getter;
        }
        public static FastPropertySetHandler GetPropertySetter(PropertyInfo propInfo)
        {
            // generates a dynamic method to generate a FastPropertySetHandler delegate
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, null, new[] { typeof(object), typeof(object) }, propInfo.DeclaringType);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            // loads the object into the stack
            ilGenerator.Emit(OpCodes.Ldarg_0);

            // loads the parameter from the stack
            ilGenerator.Emit(OpCodes.Ldarg_1);

            // cast to the proper type (unboxing if needed)
            EmitCastToReference(ilGenerator, propInfo.PropertyType);

            // calls the setter
            ilGenerator.EmitCall(OpCodes.Callvirt, propInfo.GetSetMethod(), null);

            // terminates the call
            ilGenerator.Emit(OpCodes.Ret);

            // converts the DynamicMethod to a FastPropertyGetHandler delegate to get the property
            FastPropertySetHandler setter = (FastPropertySetHandler)dynamicMethod.CreateDelegate(typeof(FastPropertySetHandler));

            return setter;
        }

        /// <summary>Emits the cast to a reference, unboxing if needed.</summary>
        /// <param name="ilGenerator">The MSIL generator.</param>
        /// <param name="type">The type to cast.</param>
        private static void EmitCastToReference(ILGenerator ilGenerator, Type type)
        {
            if (type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Castclass, type);
            }
        }

        /// <summary>Boxes a type if needed.</summary>
        /// <param name="ilGenerator">The MSIL generator.</param>
        /// <param name="type">The type.</param>
        private static void EmitBoxIfNeeded(ILGenerator ilGenerator, Type type)
        {
            if (type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, type);
            }
        }

        /// <summary>Emits code to save an integer to the evaluation stack.</summary>
        /// <param name="ilGenerator">The MSIL generator.</param>
        /// <param name="value">The value to push.</param>
        private static void EmitFastInt(ILGenerator ilGenerator, int value)
        {
            // for small integers, emit the proper opcode
            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            // for bigger values emit the short or long opcode
            if (value > -129 && value < 128)
            {
                ilGenerator.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}
