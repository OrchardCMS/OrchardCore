using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Liquid;

namespace OrchardCore.ContentFields.Handlers;

public class TextFieldHandler : ContentFieldHandler<TextField>
{
    private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    protected readonly IStringLocalizer S;

    public TextFieldHandler(
        ITypeActivatorFactory<ContentPart> contentPartFactory,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<TextFieldHandler> stringLocalizer)
    {
        _contentPartFactory = contentPartFactory;
        _liquidTemplateManager = liquidTemplateManager;
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, TextField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>();

        if (settings.Required && string.IsNullOrWhiteSpace(field.Text))
        {
            context.Fail(S["A value is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Text));
        }

        return Task.CompletedTask;
    }

    public override Task UpdatedAsync(UpdateContentFieldContext context, TextField field)
    {
        return SetValueAsync(context, field);
    }

    public override Task CreatedAsync(CreateContentFieldContext context, TextField field)
    {
        return SetValueAsync(context, field);
    }

    private async Task SetValueAsync(ContentFieldContextBase context, TextField field)
    {
        var settings = context.ContentPartFieldDefinition?.GetSettings<TextFieldSettings>();

        // Do not compute the title if the user can modify it.
        if (settings is null || settings.Type == FieldBehaviorType.Editable)
        {
            return;
        }

        if (!string.IsNullOrEmpty(settings.Pattern))
        {
            var value = await _liquidTemplateManager.RenderStringAsync(settings.Pattern, NullEncoder.Default, field,
                new Dictionary<string, FluidValue>()
                {
                    ["ContentItem"] = new ObjectValue(field.ContentItem),
                });

            field.Text = value?.Trim();

            var partActivator = _contentPartFactory.GetTypeActivator(context.PartName);
            var part = field.ContentItem.Get(partActivator.Type, context.ContentPartFieldDefinition.ContentTypePartDefinition.Name) as ContentPart;

            if (part == null)
            {
                part = partActivator.CreateInstance();
                field.ContentItem.Weld(context.ContentPartFieldDefinition.ContentTypePartDefinition.Name, part);
            }

            part.Apply(context.ContentPartFieldDefinition.Name, field);
        }
    }
}
