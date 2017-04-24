using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions.Features;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeAlterationBuilder
    {
        IFeatureInfo _feature;
        readonly string _shapeType;
        readonly string _bindingName;
        readonly IList<Action<ShapeDescriptor>> _configurations = new List<Action<ShapeDescriptor>>();

        public ShapeAlterationBuilder(IFeatureInfo feature, string shapeType)
        {
            _feature = feature;
            _bindingName = shapeType;
            var delimiterIndex = shapeType.IndexOf("__");

            if (delimiterIndex < 0)
            {
                _shapeType = shapeType;
            }
            else
            {
                _shapeType = shapeType.Substring(0, delimiterIndex);
            }
        }

        public ShapeAlterationBuilder From(IFeatureInfo feature)
        {
            _feature = feature;
            return this;
        }

        public ShapeAlterationBuilder Configure(Action<ShapeDescriptor> action)
        {
            _configurations.Add(action);
            return this;
        }

        public ShapeAlterationBuilder BoundAs(string bindingSource, Func<ShapeDescriptor, Func<DisplayContext, Task<IHtmlContent>>> binder)
        {
            // schedule the configuration
            return Configure(descriptor =>
            {
                Func<DisplayContext, Task<IHtmlContent>> target = null;

                var binding = new ShapeBinding
                {
                    ShapeDescriptor = descriptor,
                    BindingName = _bindingName,
                    BindingSource = bindingSource,
                };

                binding.BindingAsync = displayContext =>
                {
                    // when used, first realize the actual target once
                    if (target == null)
                        target = binder(binding.ShapeDescriptor);

                    // and execute the re
                    return target(displayContext);
                };

                // ShapeDescriptor.Bindings is a case insensitive dictionary
                descriptor.Bindings[_bindingName] = binding;
                descriptor.BindingSources.Add(bindingSource);
            });
        }

        public ShapeAlterationBuilder OnCreating(Action<ShapeCreatingContext> action)
        {
            return Configure(descriptor =>
            {
                var existing = descriptor.Creating ?? Enumerable.Empty<Action<ShapeCreatingContext>>();
                descriptor.Creating = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnCreated(Action<ShapeCreatedContext> action)
        {
            return Configure(descriptor =>
            {
                var existing = descriptor.Created ?? Enumerable.Empty<Action<ShapeCreatedContext>>();
                descriptor.Created = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnDisplaying(Action<ShapeDisplayContext> action)
        {
            return Configure(descriptor =>
            {
                var existing = descriptor.Displaying ?? Enumerable.Empty<Action<ShapeDisplayContext>>();
                descriptor.Displaying = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnProcessingAsync(Func<ShapeDisplayContext, Task> action)
        {
            return Configure(descriptor =>
            {
                var existing = descriptor.ProcessingAsync ?? Enumerable.Empty<Func<ShapeDisplayContext, Task>>();
                descriptor.ProcessingAsync = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder OnDisplayed(Action<ShapeDisplayContext> action)
        {
            return Configure(descriptor =>
            {
                var existing = descriptor.Displayed ?? Enumerable.Empty<Action<ShapeDisplayContext>>();
                descriptor.Displayed = existing.Concat(new[] { action });
            });
        }

        public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, PlacementInfo> action)
        {
            return Configure(descriptor =>
            {
                var next = descriptor.Placement;
                descriptor.Placement = ctx => action(ctx) ?? next(ctx);
            });
        }

        public ShapeAlterationBuilder Placement(Func<ShapePlacementContext, bool> predicate, PlacementInfo location)
        {
            return Configure(descriptor =>
            {
                var next = descriptor.Placement;
                descriptor.Placement = ctx => predicate(ctx) ? location : next(ctx);
            });
        }

        public ShapeAlteration Build()
        {
            return new ShapeAlteration(_shapeType, _feature, _configurations.ToArray());
        }
    }

    public class ShapePlacementContext
    {
        public ShapePlacementContext(string shapeType, string displayType, string differentiator, IShape zoneShape)
        {
            ShapeType = shapeType;
            DisplayType = displayType;
            Differentiator = differentiator;
            ZoneShape = ZoneShape;
        }

        public IShape ZoneShape { get; set; }
        public string ShapeType { get; set; }
        public string DisplayType { get; set; }
        public string Differentiator { get; set; }

        /// <summary>
        /// Debug information explaining where the final placement is coming from.
        /// Used by tooling.
        /// </summary>
        public string Source { get; set; }
    }
}