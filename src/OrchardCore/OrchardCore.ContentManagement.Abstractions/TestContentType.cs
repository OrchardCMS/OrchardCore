using System;
using Newtonsoft.Json;

namespace OrchardCore.ContentManagement
{
    // TODO probably actually needs to be a ContentType abstrac, we'll see.
    // because this is the thing supposed to hold the definition?
    //or do we put the definition somewhere else?
    public class TestContentType : CodeContentType
    {
        public TestContentType()
        {
            ContentTypeDefinitionRecord.Name = "TestContentType";
            ContentTypeDefinitionRecord.DisplayName = "Test Content Type";

        }

    }
}