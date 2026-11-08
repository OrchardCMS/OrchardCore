using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OrchardCore.Benchmarks.ShapePrototype;

/// <summary>
/// What renders a shape: a template writes the shape to the output. Razor and Liquid templates, and code
/// bindings, all implement it. Writing to the output instead of returning an <c>IHtmlContent</c> removes
/// one buffer per shape and lets the whole page stream.
/// </summary>
public interface IShapeTemplate
{
    ValueTask RenderAsync(ShapeRenderContext context);
}

/// <summary>
/// The context of one rendering. It is a struct: a shape rendering allocates nothing but what its template
/// allocates.
/// </summary>
public readonly struct ShapeRenderContext
{
    private readonly RenderPlan _plan;
    private readonly int _wrapperIndex;

    internal ShapeRenderContext(ShapeRenderer renderer, ShapeNode shape, TextWriter output, RenderPlan plan, int wrapperIndex)
    {
        Renderer = renderer;
        Shape = shape;
        Output = output;
        _plan = plan;
        _wrapperIndex = wrapperIndex;
    }

    public ShapeRenderer Renderer { get; }

    public ShapeNode Shape { get; }

    public TextWriter Output { get; }

    public T Model<T>() => (T)Shape.Model;

    /// <summary>
    /// Renders a child shape in place.
    /// </summary>
    public ValueTask RenderAsync(ShapeNode shape) => Renderer.RenderAsync(shape, Output);

    /// <summary>
    /// Renders a zone of the shape in place, or nothing when the zone doesn't exist.
    /// </summary>
    public ValueTask RenderZoneAsync(string name)
    {
        var zone = Shape.GetZone(name);
        return zone is null ? ValueTask.CompletedTask : Renderer.RenderAsync(zone, Output);
    }

    /// <summary>
    /// From a wrapper, renders what it wraps: the next wrapper, or the template of the shape.
    /// </summary>
    public ValueTask RenderChildContentAsync()
        => _wrapperIndex > 0
            ? _plan.Wrappers[_wrapperIndex - 1].RenderAsync(new ShapeRenderContext(Renderer, Shape, Output, _plan, _wrapperIndex - 1))
            : _plan.Template.RenderAsync(new ShapeRenderContext(Renderer, Shape, Output, _plan, -1));
}

/// <summary>
/// The result of resolving a shape key against a shape table: the template to run and the wrappers around it.
/// Computed once per key and per table.
/// </summary>
public sealed class RenderPlan
{
    public RenderPlan(IShapeTemplate template, IShapeTemplate[] wrappers, string bindingName)
    {
        Template = template;
        Wrappers = wrappers;
        BindingName = bindingName;
    }

    public IShapeTemplate Template { get; }

    /// <summary>
    /// The wrappers, the first one is the innermost.
    /// </summary>
    public IShapeTemplate[] Wrappers { get; }

    public string BindingName { get; }
}

/// <summary>
/// The bindings of a theme: what a shape table is today, reduced to what rendering needs.
/// </summary>
public sealed class TemplateTable
{
    private readonly Dictionary<string, IShapeTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string[]> _wrappers = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<ShapeKey, RenderPlan> _plans = new();
    private readonly ConcurrentDictionary<string, string[]> _fallbacks = new(StringComparer.Ordinal);

    public void Add(string bindingName, IShapeTemplate template) => _templates[bindingName] = template;

    public void AddWrapper(string shapeType, string wrapperType)
    {
        _wrappers[shapeType] = _wrappers.TryGetValue(shapeType, out var existing) ? [.. existing, wrapperType] : [wrapperType];
    }

    public RenderPlan Resolve(in ShapeKey key, string[] instanceWrappers)
    {
        if (instanceWrappers is { Length: > 0 })
        {
            // Rare: a shape with its own wrappers (e.g. from placement) is resolved without caching.
            return Compute(key, instanceWrappers);
        }

        return _plans.TryGetValue(key, out var plan) ? plan : _plans.GetOrAdd(key, static (k, t) => t.Compute(k, null), this);
    }

    private RenderPlan Compute(ShapeKey key, string[] instanceWrappers)
    {
        // The same probing as today: alternates from the highest to the lowest priority, then the type
        // and its '__' fallbacks. It runs once per key instead of once per rendered shape.
        var binding = Probe(key.MoreAlternates.Names) ?? Probe(key.Alternates.Names) ?? ProbeType(key.Type)
            ?? throw new InvalidOperationException($"The shape type '{key.Type}' is not found.");

        List<IShapeTemplate> wrappers = null;

        if (_wrappers.TryGetValue(key.Type, out var typeWrappers))
        {
            AddWrappers(ref wrappers, typeWrappers);
        }

        if (instanceWrappers is not null)
        {
            AddWrappers(ref wrappers, instanceWrappers);
        }

        return new RenderPlan(binding.Value, wrappers?.ToArray() ?? [], binding.Key);
    }

    private void AddWrappers(ref List<IShapeTemplate> wrappers, string[] names)
    {
        foreach (var name in names)
        {
            var wrapper = ProbeType(name);
            if (wrapper is not null)
            {
                (wrappers ??= []).Add(wrapper.Value.Value);
            }
        }
    }

    private KeyValuePair<string, IShapeTemplate>? Probe(string[] names)
    {
        for (var i = names.Length - 1; i >= 0; i--)
        {
            if (_templates.TryGetValue(names[i], out var template))
            {
                return new(names[i], template);
            }
        }

        return null;
    }

    private KeyValuePair<string, IShapeTemplate>? ProbeType(string type)
    {
        var fallbacks = _fallbacks.GetOrAdd(type, static type =>
        {
            var segments = new List<string>(2) { type };
            var index = type.LastIndexOf("__", StringComparison.Ordinal);
            while (index > 0)
            {
                type = type[..index];
                segments.Add(type);
                index = type.LastIndexOf("__", StringComparison.Ordinal);
            }

            return segments.ToArray();
        });

        return Probe(fallbacks is [_] ? fallbacks : Reverse(fallbacks));

        static string[] Reverse(string[] fallbacks)
        {
            var reversed = (string[])fallbacks.Clone();
            Array.Reverse(reversed);
            return reversed;
        }
    }
}

/// <summary>
/// Renders shape nodes: resolves the plan of the shape and runs it. There is no event, no context object
/// and no intermediate buffer per shape; the extension points of today's events move to the plan
/// (per type, computed once) or to the node (per instance, explicit).
/// </summary>
public sealed class ShapeRenderer
{
    private readonly TemplateTable _table;

    public ShapeRenderer(TemplateTable table)
    {
        _table = table;
    }

    public ValueTask RenderAsync(ShapeNode shape, TextWriter output)
    {
        if (shape is null)
        {
            return ValueTask.CompletedTask;
        }

        var plan = _table.Resolve(shape.Key, shape.Wrappers);

        return plan.Wrappers.Length == 0
            ? plan.Template.RenderAsync(new ShapeRenderContext(this, shape, output, plan, -1))
            : plan.Wrappers[^1].RenderAsync(new ShapeRenderContext(this, shape, output, plan, plan.Wrappers.Length - 1));
    }
}

/// <summary>
/// A code template, the equivalent of a <c>[Shape]</c> method or of a compiled Razor template.
/// </summary>
public sealed class DelegateTemplate : IShapeTemplate
{
    private readonly Func<ShapeRenderContext, ValueTask> _render;

    public DelegateTemplate(Func<ShapeRenderContext, ValueTask> render) => _render = render;

    public ValueTask RenderAsync(ShapeRenderContext context) => _render(context);
}
