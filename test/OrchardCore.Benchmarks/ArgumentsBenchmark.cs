using BenchmarkDotNet.Attributes;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Benchmarks;

[MemoryDiagnoser]
public partial class ArgumentsBenchmark
{
    [Benchmark(Description = "Generated - Create")]
#pragma warning disable CA1822 // Mark members as static
    public INamedEnumerable<object> GeneratedCreate()
#pragma warning restore CA1822
    {
        var generatedArgs = new TestGeneratedArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        return Arguments.From(generatedArgs);
    }

    [Benchmark(Description = "Generated - Enumerate All")]
#pragma warning disable CA1822
    public int GeneratedEnumerateAll()
#pragma warning restore CA1822
    {
        var generatedArgs = new TestGeneratedArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(generatedArgs);
        var count = 0;
        foreach (var item in args)
        {
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Generated - Access By Name (1 property)")]
#pragma warning disable CA1822
    public object GeneratedAccessByName()
#pragma warning restore CA1822
    {
        var generatedArgs = new TestGeneratedArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(generatedArgs);
        return args.Named["Title"];
    }

    [Benchmark(Description = "Generated - Access By Name (all 3 properties)")]
#pragma warning disable CA1822
    public (object, object, object) GeneratedAccessAllByName()
#pragma warning restore CA1822
    {
        var generatedArgs = new TestGeneratedArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(generatedArgs);
        return (args.Named["Title"], args.Named["Count"], args.Named["IsActive"]);
    }

    [Benchmark(Description = "Reflection - Create")]
#pragma warning disable CA1822
    public INamedEnumerable<object> ReflectionCreate()
#pragma warning restore CA1822
    {
        var reflectionArgs = new TestReflectionArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        return Arguments.From(reflectionArgs);
    }

    [Benchmark(Description = "Reflection - Enumerate All")]
#pragma warning disable CA1822
    public int ReflectionEnumerateAll()
#pragma warning restore CA1822
    {
        var reflectionArgs = new TestReflectionArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(reflectionArgs);
        var count = 0;
        foreach (var item in args)
        {
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Reflection - Access By Name (1 property)")]
#pragma warning disable CA1822
    public object ReflectionAccessByName()
#pragma warning restore CA1822
    {
        var reflectionArgs = new TestReflectionArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(reflectionArgs);
        return args.Named["Title"];
    }

    [Benchmark(Description = "Reflection - Access By Name (all 3 properties)")]
#pragma warning disable CA1822
    public (object, object, object) ReflectionAccessAllByName()
#pragma warning restore CA1822
    {
        var reflectionArgs = new TestReflectionArguments
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(reflectionArgs);
        return (args.Named["Title"], args.Named["Count"], args.Named["IsActive"]);
    }

    [Benchmark(Description = "Anonymous - Create")]
#pragma warning disable CA1822
    public INamedEnumerable<object> AnonymousCreate()
#pragma warning restore CA1822
    {
        var anonymousArgs = new
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        return Arguments.From(anonymousArgs);
    }

    [Benchmark(Description = "Anonymous - Enumerate All")]
#pragma warning disable CA1822
    public int AnonymousEnumerateAll()
#pragma warning restore CA1822
    {
        var anonymousArgs = new
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(anonymousArgs);
        var count = 0;
        foreach (var item in args)
        {
            count++;
        }
        return count;
    }

    [Benchmark(Description = "Anonymous - Access By Name (1 property)")]
#pragma warning disable CA1822
    public object AnonymousAccessByName()
#pragma warning restore CA1822
    {
        var anonymousArgs = new
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(anonymousArgs);
        return args.Named["Title"];
    }

    [Benchmark(Description = "Anonymous - Access By Name (all 3 properties)")]
#pragma warning disable CA1822
    public (object, object, object) AnonymousAccessAllByName()
#pragma warning restore CA1822
    {
        var anonymousArgs = new
        {
            Title = "Hello",
            Count = 42,
            IsActive = true,
        };

        var args = Arguments.From(anonymousArgs);
        return (args.Named["Title"], args.Named["Count"], args.Named["IsActive"]);
    }

    // Test with 10 properties
    [Benchmark(Description = "Generated (10 props) - Create and Access 2")]
#pragma warning disable CA1822
    public (object, object) Generated10PropsAccessTwo()
#pragma warning restore CA1822
    {
        var args = new TestGenerated10Properties
        {
            Prop1 = "Value1",
            Prop2 = "Value2",
            Prop3 = "Value3",
            Prop4 = "Value4",
            Prop5 = "Value5",
            Prop6 = "Value6",
            Prop7 = "Value7",
            Prop8 = "Value8",
            Prop9 = "Value9",
            Prop10 = "Value10",
        };
        var namedArgs = Arguments.From(args);
        return (namedArgs.Named["Prop1"], namedArgs.Named["Prop5"]);
    }

    [Benchmark(Description = "Reflection (10 props) - Create and Access 2")]
#pragma warning disable CA1822
    public (object, object) Reflection10PropsAccessTwo()
#pragma warning restore CA1822
    {
        var args = new TestReflection10Properties
        {
            Prop1 = "Value1",
            Prop2 = "Value2",
            Prop3 = "Value3",
            Prop4 = "Value4",
            Prop5 = "Value5",
            Prop6 = "Value6",
            Prop7 = "Value7",
            Prop8 = "Value8",
            Prop9 = "Value9",
            Prop10 = "Value10",
        };
        var namedArgs = Arguments.From(args);
        return (namedArgs.Named["Prop1"], namedArgs.Named["Prop5"]);
    }

    [Benchmark(Description = "Generated (10 props) - Lookup All By Name")]
#pragma warning disable CA1822
    public int Generated10PropsLookupAll()
#pragma warning restore CA1822
    {
        var args = new TestGenerated10Properties
        {
            Prop1 = "Value1",
            Prop2 = "Value2",
            Prop3 = "Value3",
            Prop4 = "Value4",
            Prop5 = "Value5",
            Prop6 = "Value6",
            Prop7 = "Value7",
            Prop8 = "Value8",
            Prop9 = "Value9",
            Prop10 = "Value10",
        };
        var namedArgs = Arguments.From(args);
        var count = 0;
        for (var i = 1; i <= 10; i++)
        {
            if (namedArgs.Named[$"Prop{i}"] != null)
            {
                count++;
            }
        }
        return count;
    }

    [GenerateArguments]
    private sealed partial class TestGeneratedArguments
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class TestReflectionArguments
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public bool IsActive { get; set; }
    }

    [GenerateArguments]
    private sealed partial class TestGenerated10Properties
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public string Prop4 { get; set; }
        public string Prop5 { get; set; }
        public string Prop6 { get; set; }
        public string Prop7 { get; set; }
        public string Prop8 { get; set; }
        public string Prop9 { get; set; }
        public string Prop10 { get; set; }
    }

    private sealed class TestReflection10Properties
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public string Prop4 { get; set; }
        public string Prop5 { get; set; }
        public string Prop6 { get; set; }
        public string Prop7 { get; set; }
        public string Prop8 { get; set; }
        public string Prop9 { get; set; }
        public string Prop10 { get; set; }
    }
}
