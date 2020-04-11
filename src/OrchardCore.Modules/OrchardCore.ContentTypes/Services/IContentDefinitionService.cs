using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.ContentTypes.Services
{
    public interface IContentDefinitionService
    {
        IEnumerable<EditTypeViewModel> LoadTypes();
        IEnumerable<EditTypeViewModel> GetTypes();
        EditTypeViewModel LoadType(string name);
        EditTypeViewModel GetType(string name);
        ContentTypeDefinition AddType(string name, string displayName);
        void RemoveType(string name, bool deleteContent);
        void AddPartToType(string partName, string typeName);
        void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName);
        void RemovePartFromType(string partName, string typeName);
        string GenerateContentTypeNameFromDisplayName(string displayName);
        string GenerateFieldNameFromDisplayName(string partName, string displayName);

        IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly);
        IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly);
        EditPartViewModel LoadPart(string name);
        EditPartViewModel GetPart(string name);
        EditPartViewModel AddPart(CreatePartViewModel partViewModel);
        void RemovePart(string name);

        IEnumerable<Type> GetFields();
        void AddFieldToPart(string fieldName, string fieldTypeName, string partName);
        void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName);
        void RemoveFieldFromPart(string fieldName, string partName);
        void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel);

        void AlterTypePart(EditTypePartViewModel partViewModel);

        void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames);
        void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames);
    }
}
