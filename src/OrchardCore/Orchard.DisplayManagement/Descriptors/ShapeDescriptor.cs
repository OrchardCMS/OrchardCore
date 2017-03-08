using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Descriptors
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
        private IEnumerable<string> _alterationKeys;
        private ConcurrentDictionary<string, FeatureShapeDescriptor> _descriptors;

        public ShapeDescriptorIndex(string shapeType, IEnumerable<string> alterationKeys, ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
        {
            ShapeType = shapeType;
            _alterationKeys = alterationKeys;
            _descriptors = descriptors;
        }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public override string BindingSource
        {
            get
            {
                ShapeBinding binding;
                return Bindings.TryGetValue(ShapeType, out binding) ? binding.BindingSource : null;
            }
        }

        public override Func<DisplayContext, Task<IHtmlContent>> Binding
        {
            get
            {
                ShapeBinding binding;
                return Bindings.TryGetValue(ShapeType, out binding) ? binding.BindingAsync : null;
            }
        }

        public override IDictionary<string, ShapeBinding> Bindings
        {
            get
            {
                return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Bindings)
                    .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase).Select(kv => kv.Last())
                    .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public override IEnumerable<Action<ShapeCreatingContext>> Creating
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Creating); }
        }
        public override IEnumerable<Action<ShapeCreatedContext>> Created
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Created); }
        }
        public override IEnumerable<Action<ShapeDisplayContext>> Displaying
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Displaying); }
        }
        public override IEnumerable<Func<ShapeDisplayContext, Task>> ProcessingAsync
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.ProcessingAsync); }
        }
        public override IEnumerable<Action<ShapeDisplayContext>> Displayed
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Displayed); }
        }

        public override Func<ShapePlacementContext, PlacementInfo> Placement
        {
            get
            {
                return (ctx =>
                {
                    PlacementInfo info = null;
                    foreach (var descriptor in _alterationKeys.Select(key => _descriptors[key]).Reverse())
                    {
                        info = descriptor.Placement(ctx);
                        if (info != null)
                            break;
                    }
                    return info ?? DefaultPlacementAction(ctx);
                });
            }
        }

        public override IList<string> Wrappers
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.Wrappers).ToList(); }
        }
        public override IList<string> BindingSources
        {
            get { return _alterationKeys.Select(key => _descriptors[key]).SelectMany(sd => sd.BindingSources).ToList(); }
        }
    }

    public class ShapeDescriptor
    {
        public ShapeDescriptor()
        {
            if (!(this is ShapeDescriptorIndex))
            {
                Creating = Enumerable.Empty<Action<ShapeCreatingContext>>();
                Created = Enumerable.Empty<Action<ShapeCreatedContext>>();
                Displaying = Enumerable.Empty<Action<ShapeDisplayContext>>();
                ProcessingAsync = Enumerable.Empty<Func<ShapeDisplayContext, Task>>();
                Displayed = Enumerable.Empty<Action<ShapeDisplayContext>>();
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

            return new PlacementInfo { Location = DefaultPlacement };
        }

        public string ShapeType { get; set; }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public virtual string BindingSource
        {
            get
            {
                ShapeBinding binding;
                return Bindings.TryGetValue(ShapeType, out binding) ? binding.BindingSource : null;
            }
        }

        public virtual Func<DisplayContext, Task<IHtmlContent>> Binding
        {
            get
            {
                return Bindings[ShapeType].BindingAsync;
            }
        }

        public virtual IDictionary<string, ShapeBinding> Bindings { get; set; }

        public virtual IEnumerable<Action<ShapeCreatingContext>> Creating { get; set; }
        public virtual IEnumerable<Action<ShapeCreatedContext>> Created { get; set; }
        public virtual IEnumerable<Action<ShapeDisplayContext>> Displaying { get; set; }
        public virtual IEnumerable<Func<ShapeDisplayContext, Task>> ProcessingAsync { get; set; }
        public virtual IEnumerable<Action<ShapeDisplayContext>> Displayed { get; set; }

        public virtual Func<ShapePlacementContext, PlacementInfo> Placement { get; set; }
        public string DefaultPlacement { get; set; }

        public virtual IList<string> Wrappers { get; set; }
        public virtual IList<string> BindingSources { get; set; }
    }

    public class ShapeBinding
    {
        public ShapeDescriptor ShapeDescriptor { get; set; }
        public string BindingName { get; set; }
        public string BindingSource { get; set; }
        public Func<DisplayContext, Task<IHtmlContent>> BindingAsync { get; set; }
    }
}