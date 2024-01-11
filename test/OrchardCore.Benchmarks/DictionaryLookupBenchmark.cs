using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;

namespace OrchardCore.Benchmark;

[MemoryDiagnoser]
public class DictionaryLookupBenchmark
{
    private const int _dictionarySize = 1000;
    private const int _percentValueToLookup = 10;

    private Dictionary<string, Guid> _dictionary;
    private FrozenDictionary<string, Guid> _frozenDictionary;
    private ImmutableDictionary<string, Guid> _immutableDictionary;

    private readonly IList<string> _locate;

    [GlobalSetup]
    public void Setup()
    {
        _dictionary = [];

        for (var i = 0; i < _dictionarySize; i++)
        {
            var key = Guid.NewGuid().ToString();

            _dictionary.TryAdd(key, Guid.NewGuid());

            if (i % 100 < _percentValueToLookup)
            {
                _locate.Add(key);
            }
        }

        _frozenDictionary = _dictionary.ToFrozenDictionary();
        _immutableDictionary = _dictionary.ToImmutableDictionary();
    }

    [Benchmark(Baseline = true)]
    public void UsingDictionary()
    {
        foreach (var item in _locate)
        {
            _dictionary.TryGetValue(item, out _);
        }
    }

    [Benchmark]
    public void UsingImmutableDictionary()
    {
        foreach (var item in _locate)
        {
            _immutableDictionary.TryGetValue(item, out _);
        }
    }

    [Benchmark]
    public void UsingFrozenDictionary()
    {
        foreach (var item in _locate)
        {
            _frozenDictionary.TryGetValue(item, out _);
        }
    }
}
