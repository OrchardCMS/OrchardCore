using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public abstract class ContentFieldDriver<TField> : IContentFieldDriver where TField : ContentField, new()
    {
        private static readonly ContentFieldInfo _contentFieldInfo;
        static ContentFieldDriver()
        {
            _contentFieldInfo = new ContentFieldInfo
            {
                FieldTypeName = typeof(TField).Name,
                Factory = partFieldDefinition => new TField()
            };
        }

        ContentFieldInfo IContentFieldDriver.GetFieldInfo()
        {
            return _contentFieldInfo;
        }
    }
}