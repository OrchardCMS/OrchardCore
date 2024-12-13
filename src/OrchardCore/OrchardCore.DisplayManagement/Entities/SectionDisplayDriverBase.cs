using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;

namespace OrchardCore.DisplayManagement.Entities;

public abstract class SectionDisplayDriverBase<TModel, TSection> : DisplayDriver<TModel>
    where TSection : new()
    where TModel : class, IEntity
{
    /// <summary>
    /// Gets the property name that the <typeparamref name="TSection"/> is stored in the <typeparamref name="TModel"/>.
    /// </summary>
    /// <remarks>
    /// Overriding this property allows changing the name of the property that the section is stored in from the
    /// default, which is <c>typeof(TSection).Name</c>.
    /// </remarks>
    protected virtual string PropertyName
        => typeof(TSection).Name;

    public override Task<IDisplayResult> DisplayAsync(TModel model, BuildDisplayContext context)
    {
        var section = GetSection(model);

        return DisplayAsync(model, section, context);
    }

    public virtual Task<IDisplayResult> DisplayAsync(TModel model, TSection section, BuildDisplayContext context)
    {
        return Task.FromResult(Display(model, section, context));
    }

    public virtual IDisplayResult Display(TModel model, TSection section, BuildDisplayContext context)
        => NullShapeResult();

    public override Task<IDisplayResult> EditAsync(TModel model, BuildEditorContext context)
    {
        var section = GetSection(model);

        return EditAsync(model, section, context);
    }

    public virtual Task<IDisplayResult> EditAsync(TModel model, TSection section, BuildEditorContext context)
    {
        return Task.FromResult(Edit(model, section, context));
    }

    public virtual IDisplayResult Edit(TModel model, TSection section, BuildEditorContext context)
        => NullShapeResult();

    public override async Task<IDisplayResult> UpdateAsync(TModel model, UpdateEditorContext context)
    {
        var section = GetSection(model);

        var result = await UpdateAsync(model, section, context);

        if (result == null)
        {
            return null;
        }

        if (context.Updater.ModelState.IsValid)
        {
            model.Properties[PropertyName] = JObject.FromObject(section);
        }

        return result;
    }

    public virtual Task<IDisplayResult> UpdateAsync(TModel model, TSection section, UpdateEditorContext context)
        => EditAsync(model, section, context);

    private TSection GetSection(TModel model)
        => model.Properties.TryGetPropertyValue(PropertyName, out var section)
        ? section.ToObject<TSection>()
        : new TSection();

    protected override void BuildPrefix(TModel model, string htmlFieldPrefix)
    {
        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = $"{htmlFieldPrefix}.{ModelName}.{PropertyName}";
        }
        else
        {
            Prefix = $"{ModelName}.{PropertyName}";
        }
    }
}
