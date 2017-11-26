//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using GraphQL.Types;
//using OrchardCore.Apis.GraphQL;

//namespace OrchardCore.Contents.Apis.GraphQL.Queries.Types
//{
//    public class ContentPartInterfaceType : InterfaceGraphType
//    {
//        public ContentPartInterfaceType(ContentSchema schema) {
//            Name = "ContentPart";

//            foreach(var type in schema.AllTypes.OfType<ContentPartAutoRegisteringObjectGraphType>()) {
//                AddPossibleType(type);
//            }
//        }
//    }
//}
