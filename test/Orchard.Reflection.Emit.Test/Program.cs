using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Test
{
    public class Program
    {
        public class TestBase
        {
            public string Name { get; set; }
           
            public int Run(int end)
            {
                var sum = 0;
                for (var i = 0; i < end; i++)
                {
                    sum++;
                }
                return sum;
            }
        }
        public void Main(string[] args)
        {
            EvaluateMethod("direct - object creation", delegate () {
                TestBase p = new TestBase();
            }, 1000000);

            FastCreateInstanceHandler creator = ObjectReflection.GetInstanceCreator(typeof(TestBase));

            EvaluateMethod("dynamic method - object creation", delegate () {
                TestBase p = (TestBase)creator.Invoke();
            }, 1000000);

            EvaluateMethod("reflection - object creation", delegate () {
                TestBase p = (TestBase)Activator.CreateInstance(typeof(TestBase));
            }, 1000000);

            // property get

            Console.WriteLine("------ Property Get ------");

            TestBase prod = new TestBase();
            prod.Name = "GVX";

            EvaluateMethod("direct - property get", delegate () {
                var name = prod.Name;
            }, 1000000);

            PropertyInfo propInfo = typeof(TestBase).GetProperty("Name");
            FastPropertyGetHandler getter = ObjectReflection.GetPropertyGetter(propInfo);

            EvaluateMethod("dynamic method - property get", delegate () {
                string name = (string)getter(prod);
            }, 1000000);

            EvaluateMethod("reflection - property get", delegate () {
                string name = (string)propInfo.GetValue(prod, null);
            }, 1000000);

            // property set

            Console.WriteLine("------ Property Set ------");

            EvaluateMethod("direct - property set", delegate () {
                prod.Name = "Rick";
            }, 1000000);

            FastPropertySetHandler setter = ObjectReflection.GetPropertySetter(propInfo);

            EvaluateMethod("dynamic method - property set", delegate () {
                setter(prod, "Rick");
            }, 1000000);

            EvaluateMethod("reflection - property set", delegate () {
                propInfo.SetValue(prod, "Rick", null);
            }, 1000000);

            // instance method call

            Console.WriteLine("------ Instance Method Call ------");

            EvaluateMethod("direct - instance method call", delegate () {
                int result = prod.Run(10);
            }, 1000000);

            MethodInfo methodInfo1 = typeof(TestBase).GetMethod("Run");
            FastInvokeHandler fastInvoker1 = methodInfo1.DynamicMethod();

            EvaluateMethod("dynamic method - instance method call", delegate () {
                int result = (int)fastInvoker1(prod, new object[] { 10 });
            }, 1000000);

            EvaluateMethod("reflection - instance method call", delegate () {
                int result = (int)methodInfo1.Invoke(prod, new object[] { 10 });
            }, 1000000);
        }
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
    }
}
