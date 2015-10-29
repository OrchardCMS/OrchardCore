using System;
using System.Linq;
using System.Reflection;

namespace Orchard.Events {
    public static class DelegateHelper {
        public static Func<T, object[], object> CreateDelegate<T>(MethodInfo method) {
            return CreateDelegate<T>(typeof(T), method);
        }

        /// <summary>
        /// Creates a strongly-typed dynamic delegate that represents a given method on a given target type.
        /// </summary>
        /// <remarks>
        /// Provided method must be valid for the type provided as targetType.
        /// </remarks>
        /// <typeparam name="T">Type of the delegate target (first argument) object. Needs to be assignable from the type provided as targetType parameter.</typeparam>
        /// <param name="targetType">Delegate target type.</param>
        /// <param name="method">Method that the delegate represents.</param>
        /// <returns>Strongly-typed delegate representing the given method. 
        /// First argument is the target of the delegate. 
        /// Second argument describes call arguments.
        ///  </returns>
        public static Func<T, object[], object> CreateDelegate<T>(Type targetType, MethodInfo method) {
            var parameters = method.ReturnType == typeof(void)
                ? new[] { targetType }.Concat(method.GetParameters().Select(p => p.ParameterType)).ToArray()
                : new[] { targetType }.Concat(method.GetParameters().Select(p => p.ParameterType)).Concat(new[] { method.ReturnType }).ToArray();

            // First fetch the generic form
            MethodInfo genericHelper = method.ReturnType == typeof(void)
                ? typeof(DelegateHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).First(m => m.Name == "BuildAction" && m.GetGenericArguments().Count() == parameters.Length)
                : typeof(DelegateHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).First(m => m.Name == "BuildFunc" && m.GetGenericArguments().Count() == parameters.Length);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(parameters);

            // Now call it. The null argument is because it's a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });
            return (Func<T, object[], object>)ret;
        }

        static Func<object, object[], object> BuildFunc<T, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, TRet>)method.CreateDelegate(typeof(Func<T, TRet>));
#else
            var func = (Func<T, TRet>)Delegate.CreateDelegate(typeof(Func<T, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, TRet>)method.CreateDelegate(typeof(Func<T, T1, TRet>));
#else
            var func = (Func<T, T1, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, TRet>));
#else
            var func = (Func<T, T1, T2, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, TRet>));
#else
            var func = (Func<T, T1, T2, T3, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, T6, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, T6, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, T6, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, T6, T7, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7], (T9)p[8]);
            return ret;
        }

        static Func<object, object[], object> BuildFunc<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(MethodInfo method) {
#if DNXCORE50
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>)method.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>));
#else
            var func = (Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>)Delegate.CreateDelegate(typeof(Func<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>), method);
#endif
            Func<object, object[], object> ret = (target, p) => func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7], (T9)p[8], (T10)p[9]);
            return ret;
        }

        static Func<object, object[], object> BuildAction<T>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T>)method.CreateDelegate(typeof(Action<T>));
#else
            var func = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1>)method.CreateDelegate(typeof(Action<T, T1>));
#else
            var func = (Action<T, T1>)Delegate.CreateDelegate(typeof(Action<T, T1>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2>)method.CreateDelegate(typeof(Action<T, T1, T2>));
#else
            var func = (Action<T, T1, T2>)Delegate.CreateDelegate(typeof(Action<T, T1, T2>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3>)method.CreateDelegate(typeof(Action<T, T1, T2, T3>));
#else
            var func = (Action<T, T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4>));
#else
            var func = (Action<T, T1, T2, T3, T4>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5, T6>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5, T6>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5, T6>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5, T6, T7>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5, T6, T7, T8>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7], (T9)p[8]); return null; };
            return ret;
        }

        static Func<object, object[], object> BuildAction<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MethodInfo method) {
#if DNXCORE50
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)method.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>));
#else
            var func = (Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)Delegate.CreateDelegate(typeof(Action<T, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>), method);
#endif
            Func<object, object[], object> ret = (target, p) => { func((T)target, (T1)p[0], (T2)p[1], (T3)p[2], (T4)p[3], (T5)p[4], (T6)p[5], (T7)p[6], (T8)p[7], (T9)p[8], (T10)p[9]); return null; };
            return ret;
        }
    }
}
