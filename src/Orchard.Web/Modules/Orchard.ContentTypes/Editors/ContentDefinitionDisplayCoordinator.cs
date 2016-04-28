using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;

using Orchard.DisplayManagement.ModelBinding;
using Orchard.DependencyInjection;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;

namespace Orchard.ContentTypes.Editors
{

    public class BuildContentDefinitionEditorContext<TBuilder> : BuildEditorContext
    {
        public BuildContentDefinitionEditorContext(
            TBuilder builder,
            IShape model,
            string groupId,
            IShapeFactory shapeFactory,
            IShape layout,
            IUpdateModel updater)
            : base(model, groupId, shapeFactory, layout, updater)
        {
            Builder = builder;
        }

        public TBuilder Builder { get; private set; }
    }

    public class UpdateContentDefinitionEditorContext<TBuilder> : UpdateEditorContext
    {
        public UpdateContentDefinitionEditorContext(
            TBuilder builder,
            IShape model,
            string groupId,
            IShapeFactory shapeFactory,
            IShape layout,
            IUpdateModel updater)
            : base(model, groupId, shapeFactory, layout, updater)
        {
            Builder = builder;
        }

        public TBuilder Builder { get; private set; }
    }

    public class BuildTypeEditorContext : BuildContentDefinitionEditorContext<ContentTypeDefinitionBuilder> {
        public BuildTypeEditorContext(
                ContentTypeDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdateTypeEditorContext : UpdateContentDefinitionEditorContext<ContentTypeDefinitionBuilder>
    {
        public UpdateTypeEditorContext(
                ContentTypeDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class BuildPartEditorContext : BuildContentDefinitionEditorContext<ContentTypePartDefinitionBuilder>
    {
        public BuildPartEditorContext(
                ContentTypePartDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdatePartEditorContext : UpdateContentDefinitionEditorContext<ContentTypePartDefinitionBuilder>
    {
        public UpdatePartEditorContext(
                ContentTypePartDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }


    public class BuildFieldEditorContext : BuildContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public BuildFieldEditorContext(
                ContentPartFieldDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public class UpdateFieldEditorContext : UpdateContentDefinitionEditorContext<ContentPartFieldDefinitionBuilder>
    {
        public UpdateFieldEditorContext(
                ContentPartFieldDefinitionBuilder builder,
                IShape model,
                string groupId,
                IShapeFactory shapeFactory,
                IShape layout,
                IUpdateModel updater)
            : base(builder, model, groupId, shapeFactory, layout, updater)
        {
        }
    }

    public interface IContentDefinitionDisplayHandler : IDependency
    {
        Task BuildTypeDisplayAsync(ContentTypeDefinition definition, BuildDisplayContext context);
        Task BuildTypeEditorAsync(ContentTypeDefinition definition, BuildTypeEditorContext context);
        Task UpdateTypeEditorAsync(ContentTypeDefinition definition, UpdateTypeEditorContext context);

        Task BuildPartDisplayAsync(ContentTypePartDefinition definition, BuildDisplayContext context);
        Task BuildPartEditorAsync(ContentTypePartDefinition definition, BuildPartEditorContext context);
        Task UpdatePartEditorAsync(ContentTypePartDefinition definition, UpdatePartEditorContext context);

        Task BuildFieldDisplayAsync(ContentPartFieldDefinition definition, BuildDisplayContext context);
        Task BuildFieldEditorAsync(ContentPartFieldDefinition definition, BuildFieldEditorContext context);
        Task UpdateFieldEditorAsync(ContentPartFieldDefinition definition, UpdateFieldEditorContext context);
    }

    public interface IContentTypeDefinitionDisplayDriver : IDisplayDriver<ContentTypeDefinition, BuildDisplayContext, BuildTypeEditorContext, UpdateTypeEditorContext>, IDependency
    {
    }

    public interface IContentTypePartDefinitionDisplayDriver : IDisplayDriver<ContentTypePartDefinition, BuildDisplayContext, BuildPartEditorContext, UpdatePartEditorContext>, IDependency
    {
    }

    public interface IContentPartFieldDefinitionDisplayDriver : IDisplayDriver<ContentPartFieldDefinition, BuildDisplayContext, BuildFieldEditorContext, UpdateFieldEditorContext>, IDependency
    {
    }

    public class ContentDefinitionDisplayCoordinator : IContentDefinitionDisplayHandler
    {
        private readonly IEnumerable<IContentTypeDefinitionDisplayDriver> _typeDisplayDrivers;
        private readonly IEnumerable<IContentTypePartDefinitionDisplayDriver> _partDisplayDrivers;
        private readonly IEnumerable<IContentPartFieldDefinitionDisplayDriver> _fieldDisplayDrivers;

        public ContentDefinitionDisplayCoordinator(
            IEnumerable<IContentTypeDefinitionDisplayDriver> typeDisplayDrivers,
            IEnumerable<IContentTypePartDefinitionDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartFieldDefinitionDisplayDriver> fieldDisplayDrivers,
            ILogger<IContentDefinitionDisplayHandler> logger)
        {
            _fieldDisplayDrivers = fieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            _typeDisplayDrivers = typeDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public Task BuildTypeDisplayAsync(ContentTypeDefinition model, BuildDisplayContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildTypeEditorAsync(ContentTypeDefinition model, BuildTypeEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateTypeEditorAsync(ContentTypeDefinition model, UpdateTypeEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartDisplayAsync(ContentTypePartDefinition model, BuildDisplayContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartEditorAsync(ContentTypePartDefinition model, BuildPartEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartEditorAsync(ContentTypePartDefinition model, UpdatePartEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildFieldDisplayAsync(ContentPartFieldDefinition model, BuildDisplayContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildFieldEditorAsync(ContentPartFieldDefinition model, BuildFieldEditorContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateFieldEditorAsync(ContentPartFieldDefinition model, UpdateFieldEditorContext context)
        {
            return _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}