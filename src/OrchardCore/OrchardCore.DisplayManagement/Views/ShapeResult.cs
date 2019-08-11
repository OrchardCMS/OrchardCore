using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Views
{
    public class ShapeResult : IDisplayResult
    {
        private string _defaultLocation;
        private IDictionary<string, string> _otherLocations;
        private string _differentiator;
        private string _prefix;
        private string _cacheId;
        private readonly string _shapeType;
        private readonly Func<IBuildShapeContext, Task<IShape>> _shapeBuilder;
        private readonly Func<IShape, Task> _processing;
        private Action<CacheContext> _cache;
        private string _groupId;
        private Action<ShapeDisplayContext> _displaying;

        public ShapeResult(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder)
            : this(shapeType, shapeBuilder, null)
        {
        }

        public ShapeResult(string shapeType, Func<IBuildShapeContext, Task<IShape>> shapeBuilder, Func<IShape, Task> processing)
        {
            // The shape type is necessary before the shape is created as it will drive the placement
            // resolution which itself can prevent the shape from being created.

            _shapeType = shapeType;
            _shapeBuilder = shapeBuilder;
            _processing = processing;
        }

        public Task ApplyAsync(BuildDisplayContext context)
        {
            return ApplyImplementationAsync(context, context.DisplayType);
        }

        public Task ApplyAsync(BuildEditorContext context)
        {
            return ApplyImplementationAsync(context, "Edit");
        }

        private async Task ApplyImplementationAsync(BuildShapeContext context, string displayType)
        {
            // If no location is set from the driver, use the one from the context
            if (String.IsNullOrEmpty(_defaultLocation))
            {
                _defaultLocation = context.DefaultZone;
            }

            // Look into specific implementations of placements (like placement.info files)
            var placement = context.FindPlacement(_shapeType, _differentiator, displayType, context);

            // Look for mapped display type locations
            if (_otherLocations != null)
            {
                string displayTypePlacement;
                if (_otherLocations.TryGetValue(displayType, out displayTypePlacement))
                {
                    _defaultLocation = displayTypePlacement;
                }
            }

            // If no placement is found, use the default location
            if (placement == null)
            {
                placement = new PlacementInfo() { Location = _defaultLocation };
            }

            if (placement.Location == null)
            {
                // If a placement was found without actual location, use the default.
                // It can happen when just setting alternates or wrappers for instance.
                placement.Location = _defaultLocation;
            }

            if (placement.DefaultPosition == null)
            {
                placement.DefaultPosition = context.DefaultPosition;
            }


            // If there are no placement or it's explicitely noop then stop rendering execution
            if (String.IsNullOrEmpty(placement.Location) || placement.Location == "-")
            {
                return;
            }

            // Parse group placement.
            _groupId = placement.GetGroup() ?? _groupId;

            // If the shape's group doesn't match the currently rendered one, return
            if (!String.Equals(context.GroupId ?? "", _groupId ?? "", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var newShape = Shape = await _shapeBuilder(context);

            // Ignore it if the driver returned a null shape.
            if (newShape == null)
            {
                return;
            }

            ShapeMetadata newShapeMetadata = newShape.Metadata;
            newShapeMetadata.Prefix = _prefix;
            newShapeMetadata.Name = _differentiator ?? _shapeType;
            newShapeMetadata.DisplayType = displayType;
            newShapeMetadata.PlacementSource = placement.Source;
            newShapeMetadata.Tab = placement.GetTab();
            newShapeMetadata.Type = _shapeType;

            if (_displaying != null)
            {
                newShapeMetadata.OnDisplaying(_displaying);
            }

            // The _processing callback is used to delay execution of costly initialization
            // that can be prevented by caching
            if (_processing != null)
            {
                newShapeMetadata.OnProcessing(_processing);
            }

            // Apply cache settings
            if (!String.IsNullOrEmpty(_cacheId) && _cache != null)
            {
                _cache(newShapeMetadata.Cache(_cacheId));
            }

            // If a specific shape is provided, remove all previous alternates and wrappers.
            if (!String.IsNullOrEmpty(placement.ShapeType))
            {
                newShapeMetadata.Type = placement.ShapeType;
                newShapeMetadata.Alternates.Clear();
                newShapeMetadata.Wrappers.Clear();
            }

            if (placement != null)
            {
                if (placement.Alternates != null)
                {
                    newShapeMetadata.Alternates.AddRange(placement.Alternates);
                }

                if (placement.Wrappers != null)
                {
                    newShapeMetadata.Wrappers.AddRange(placement.Wrappers);
                }
            }

            dynamic parentShape = context.Shape;

            if (placement.IsLayoutZone())
            {
                parentShape = context.Layout;
            }

            var position = placement.GetPosition();
            var zones = placement.GetZones();

            foreach (var zone in zones)
            {
                if (parentShape == null)
                {
                    break;
                }

                var zoneProperty = parentShape.Zones;
                if (zoneProperty != null)
                {
                    // parentShape is a ZoneHolding
                    parentShape = zoneProperty[zone];
                }
                else
                {
                    // try to access it as a member
                    parentShape = parentShape[zone];
                }
            }

            position = !String.IsNullOrEmpty(position) ? position : null;

            if (parentShape is ZoneOnDemand zoneOnDemand)
            {
                await zoneOnDemand.AddAsync(newShape, position);
            }
            else
            {
                parentShape.Add(newShape, position);
            }
        }

        /// <summary>
        /// Sets the prefix of the form elements rendered in the shape.
        /// </summary>
        /// <remarks>
        /// The goal is to isolate each shape when edited together.
        /// </remarks>
        public ShapeResult Prefix(string prefix)
        {
            _prefix = prefix;
            return this;
        }

        /// <summary>
        /// Sets the default location of the shape when no specific placement applies.
        /// </summary>
        public ShapeResult Location(string location)
        {
            _defaultLocation = location;
            return this;
        }

        /// <summary>
        /// Sets the location to use for a matching display type.
        /// </summary>
        public ShapeResult Location(string displayType, string location)
        {
            if (_otherLocations == null)
            {
                _otherLocations = new Dictionary<string, string>(2);
            }

            _otherLocations[displayType] = location;
            return this;
        }

        /// <summary>
        /// Sets the location to use for a matching display type.
        /// </summary>
        public ShapeResult Displaying(Action<ShapeDisplayContext> displaying)
        {
            _displaying = displaying;

            return this;
        }

        /// <summary>
        /// Sets a discriminator that is used to find the location of the shape when two shapes of the same type are displayed.
        /// </summary>
        public ShapeResult Differentiator(string differentiator)
        {
            _differentiator = differentiator;
            return this;
        }

        /// <summary>
        /// Sets the group identifier the shape will be rendered in.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public ShapeResult OnGroup(string groupId)
        {
            _groupId = groupId;
            return this;
        }

        /// <summary>
        /// Sets the caching properties of the shape to render.
        /// </summary>
        public ShapeResult Cache(string cacheId, Action<CacheContext> cache = null)
        {
            _cacheId = cacheId;
            _cache = cache;
            return this;
        }

        public IShape Shape { get; private set; }
    }
}
