using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Metadata;

public static class ContentDefinitionManagerExtensions
{
    public static async Task AlterTypeDefinitionAsync(this IContentDefinitionManager manager, string name, Func<ContentTypeDefinitionBuilder, Task> alterationAsync)
    {
        var typeDefinition = (await manager.LoadTypeDefinitionAsync(name)) ?? new ContentTypeDefinition(name, name.CamelFriendly());
        var builder = new ContentTypeDefinitionBuilder(typeDefinition);
        await alterationAsync(builder);
        await manager.StoreTypeDefinitionAsync(builder.Build());
    }

    public static async Task AlterTypeDefinitionAsync(this IContentDefinitionManager manager, string name, Action<ContentTypeDefinitionBuilder> alteration)
    {
        var typeDefinition = (await manager.LoadTypeDefinitionAsync(name)) ?? new ContentTypeDefinition(name, name.CamelFriendly());
        var builder = new ContentTypeDefinitionBuilder(typeDefinition);
        alteration(builder);
        await manager.StoreTypeDefinitionAsync(builder.Build());
    }

    public static async Task AlterPartDefinitionAsync(this IContentDefinitionManager manager, string name, Func<ContentPartDefinitionBuilder, Task> alterationAsync)
    {
        var partDefinition = (await manager.LoadPartDefinitionAsync(name)) ?? new ContentPartDefinition(name);
        var builder = new ContentPartDefinitionBuilder(partDefinition);
        await alterationAsync(builder);
        await manager.StorePartDefinitionAsync(builder.Build());
    }

    public static async Task AlterPartDefinitionAsync(this IContentDefinitionManager manager, string name, Action<ContentPartDefinitionBuilder> alteration)
    {
        var partDefinition = (await manager.LoadPartDefinitionAsync(name)) ?? new ContentPartDefinition(name);
        var builder = new ContentPartDefinitionBuilder(partDefinition);
        alteration(builder);
        await manager.StorePartDefinitionAsync(builder.Build());
    }

    /// <summary>
    /// Migrate existing ContentPart settings to WithSettings<typeparamref name="TSettings"/>
    /// This method will be removed in a future release.
    /// </summary>
    /// <typeparam name="TPart"></typeparam>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="manager"></param>
    public static async Task MigratePartSettingsAsync<TPart, TSettings>(this IContentDefinitionManager manager)
        where TPart : ContentPart where TSettings : class
    {
        var contentTypes = await manager.LoadTypeDefinitionsAsync();

        foreach (var contentType in contentTypes)
        {
            var partDefinition = contentType.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(TPart).Name);
            if (partDefinition != null)
            {
                var existingSettings = partDefinition.Settings.ToObject<TSettings>();

                // Remove existing properties from JObject
                var properties = typeof(TSettings).GetProperties();
                foreach (var property in properties)
                {
                    partDefinition.Settings.Remove(property.Name);
                }

                // Apply existing settings to type definition WithSettings<T>
                await manager.AlterTypeDefinitionAsync(contentType.Name, typeBuilder =>
                {
                    typeBuilder.WithPart(partDefinition.Name, partBuilder =>
                    {
                        partBuilder.WithSettings(existingSettings);
                    });

                    return Task.CompletedTask;
                });
            }
        }
    }

    /// <summary>
    /// Migrate existing ContentField settings to WithSettings<typeparamref name="TSettings"/>
    /// This method will be removed in a future release.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <typeparam name="TSettings"></typeparam>
    /// <param name="manager"></param>
    public static async Task MigrateFieldSettingsAsync<TField, TSettings>(this IContentDefinitionManager manager)
        where TField : ContentField where TSettings : class
    {
        var partDefinitions = await manager.LoadPartDefinitionsAsync();

        foreach (var partDefinition in partDefinitions)
        {
            await manager.AlterPartDefinitionAsync(partDefinition.Name, partBuilder =>
            {
                foreach (var fieldDefinition in partDefinition.Fields.Where(x => x.FieldDefinition.Name == typeof(TField).Name))
                {
                    var existingFieldSettings = fieldDefinition.Settings.ToObject<TSettings>();

                    // Do this before creating builder, so settings are removed from the builder settings object.
                    // Remove existing properties from JObject
                    var fieldSettingsProperties = existingFieldSettings.GetType().GetProperties();
                    var hasSetting = false;
                    foreach (var property in fieldSettingsProperties)
                    {
                        if (fieldDefinition.Settings.ContainsKey(property.Name))
                        {
                            hasSetting = true;
                            fieldDefinition.Settings.Remove(property.Name);
                        }
                    }

                    // Only include settings if the definition already has at least one of these settings.
                    if (hasSetting)
                    {
                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.WithSettings(existingFieldSettings);
                        });
                    }
                }
            });
        }
    }
}
