using System;

namespace OrchardCore.Data.Documents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FileDocumentStoreAttribute : Attribute
    {
        public string FileName { get; set; }
    }
}
