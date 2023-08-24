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
        private readonly List<FeatureShapeDescriptor> _alternationDescriptors;
        private readonly List<string> _wrappers;
        private readonly List<string> _bindingSources;
        private readonly Dictionary<string, ShapeBinding> _bindings;
        private readonly List<Func<ShapeCreatingContext, Task>> _creatingAsync;
        private readonly List<Func<ShapeCreatedContext, Task>> _createdAsync;
        private readonly List<Func<ShapeDisplayContext, Task>> _displayingAsync;
        private readonly List<Func<ShapeDisplayContext, Task>> _processingAsync;
        private readonly List<Func<ShapeDisplayContext, Task>> _displayedAsync;

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

        public override IEnumerable<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

        public override IEnumerable<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

        public override IEnumerable<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

        public override IEnumerable<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

        public override IEnumerable<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

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

        public override IList<string> Wrappers => _wrappers;

        public override IList<string> BindingSources => _bindingSources;
    }

    public class ShapeDescriptor
    {
        public ShapeDescriptor()
        {
            if (this is not ShapeDescriptorIndex)
            {
                CreatingAsync = Enumerable.Empty<Func<ShapeCreatingContext, Task>>();
                CreatedAsync = Enumerable.Empty<Func<ShapeCreatedContext, Task>>();
                DisplayingAsync = Enumerable.Empty<Func<ShapeDisplayContext, Task>>();
                ProcessingAsync = Enumerable.Empty<Func<ShapeDisplayContext, Task>>();
                DisplayedAsync = Enumerable.Empty<Func<ShapeDisplayContext, Task>>();
                Wrappers = new List<string>();
                BindingSources = new List<string>();
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);
            }

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

        public virtual IDictionary<string, ShapeBinding> Bindings { get; set; }

        public virtual IEnumerable<Func<ShapeCreatingContext, Task>> CreatingAsync { get; set; }
        public virtual IEnumerable<Func<ShapeCreatedContext, Task>> CreatedAsync { get; set; }
        public virtual IEnumerable<Func<ShapeDisplayContext, Task>> DisplayingAsync { get; set; }
        public virtual IEnumerable<Func<ShapeDisplayContext, Task>> ProcessingAsync { get; set; }
        public virtual IEnumerable<Func<ShapeDisplayContext, Task>> DisplayedAsync { get; set; }

        public virtual Func<ShapePlacementContext, PlacementInfo> Placement { get; set; }
        public string DefaultPlacement { get; set; }

        public virtual IList<string> Wrappers { get; set; }
        public virtual IList<string> BindingSources { get; set; }
    }

    public class ShapeBinding
    {
        public string BindingName { get; set; }
        public string BindingSource { get; set; }
        public virtual Func<DisplayContext, Task<IHtmlContent>> BindingAsync { get; set; }
    }
}
