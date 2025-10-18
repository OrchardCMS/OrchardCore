using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.Benchmarks.Support;

public class OriginalShapeDescriptor
{
    public OriginalShapeDescriptor()
    {
        Placement = DefaultPlacementAction;
    }

    protected PlacementInfo DefaultPlacementAction(ShapePlacementContext context)
    {
        // A null default placement means no default placement is specified
        if (DefaultPlacement == null)
        {
            return null;
        }

        return new PlacementInfo
        {
            Location = DefaultPlacement,
        };
    }

    public string ShapeType { get; set; }

    /// <summary>
    /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
    /// troubleshooting.
    /// </summary>
    public virtual string BindingSource =>
        Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

    public virtual Func<DisplayContext, Task<IHtmlContent>> Binding =>
        Bindings[ShapeType].BindingAsync;

    public virtual IDictionary<string, ShapeBinding> Bindings { get; } = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);

    public virtual IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync { get; set; } = [];
    public virtual IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync { get; set; } = [];
    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync { get; set; } = [];
    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync { get; set; } = [];
    public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync { get; set; } = [];

    public virtual Func<ShapePlacementContext, PlacementInfo> Placement { get; set; }
    public string DefaultPlacement { get; set; }

    public virtual IReadOnlyList<string> Wrappers { get; set; } = [];
    public virtual IReadOnlyList<string> BindingSources { get; set; } = [];
}

