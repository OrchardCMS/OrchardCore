using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.DisplayManagement.Descriptors
{
    public class FeatureShapeDescriptor : ShapeDescriptor
    {
        public FeatureShapeDescriptor(IFeatureInfo feature, string shapeType)
        {
            Feature = feature;
            ShapeType = shapeType;
        }

        public IFeatureInfo Feature { get; private set; }
    }

    public class ShapeDescriptorIndex : ShapeDescriptor
    {
        private readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _descriptors;
        private readonly IReadOnlyList<FeatureShapeDescriptor> _alternationDescriptors;
        private readonly IReadOnlyList<string> _wrappers;
        private readonly IReadOnlyList<string> _bindingSources;
        private readonly Dictionary<string, ShapeBinding> _bindings;
        private readonly IReadOnlyList<Func<ShapeCreatingContext, Task>> _creatingAsync;
        private readonly IReadOnlyList<Func<ShapeCreatedContext, Task>> _createdAsync;
        private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _displayingAsync;
        private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _processingAsync;
        private readonly IReadOnlyList<Func<ShapeDisplayContext, Task>> _displayedAsync;

        public ShapeDescriptorIndex(
            string shapeType,
            IEnumerable<string> alterationKeys,
            ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
        {
            ShapeType = shapeType;
            _descriptors = descriptors;

            // pre-calculate as much as we can
            _alternationDescriptors = alterationKeys
                .Select(key => _descriptors[key])
                .ToList();

            _wrappers = _alternationDescriptors
                .SelectMany(sd => sd.Wrappers)
                .ToList();

            _bindingSources = _alternationDescriptors
                .SelectMany(sd => sd.BindingSources)
                .ToList();

            _bindings = _alternationDescriptors
                .SelectMany(sd => sd.Bindings)
                .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => kv.Last())
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            _creatingAsync = _alternationDescriptors
                .SelectMany(sd => sd.CreatingAsync)
                .ToList();

            _createdAsync = _alternationDescriptors
                .SelectMany(sd => sd.CreatedAsync)
                .ToList();

            _displayingAsync = _alternationDescriptors
                .SelectMany(sd => sd.DisplayingAsync)
                .ToList();

            _processingAsync = _alternationDescriptors
                .SelectMany(sd => sd.ProcessingAsync)
                .ToList();

            _displayedAsync = _alternationDescriptors
                .SelectMany(sd => sd.DisplayedAsync)
                .ToList();
        }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public override string BindingSource =>
            Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

        public override Func<DisplayContext, Task<IHtmlContent>> Binding =>
            Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingAsync : null;

        public override IDictionary<string, ShapeBinding> Bindings => _bindings;

        public override IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

        public override IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

        public override Func<ShapePlacementContext, PlacementInfo> Placement => CalculatePlacement;

        private PlacementInfo CalculatePlacement(ShapePlacementContext ctx)
        {
            PlacementInfo info = null;
            for (var i = _alternationDescriptors.Count - 1; i >= 0; i--)
            {
                var descriptor = _alternationDescriptors[i];
                info = descriptor.Placement(ctx);
                if (info != null)
                {
                    break;
                }
            }

            return info ?? DefaultPlacementAction(ctx);
        }

        public override IReadOnlyList<string> Wrappers => _wrappers;

        public override IReadOnlyList<string> BindingSources => _bindingSources;
    }

    public class ShapeDescriptor
    {
        public ShapeDescriptor()
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
                Location = DefaultPlacement
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

    public class ShapeBinding
    {
        public string BindingName { get; set; }
        public string BindingSource { get; set; }
        public virtual Func<DisplayContext, Task<IHtmlContent>> BindingAsync { get; set; }
    }
}
