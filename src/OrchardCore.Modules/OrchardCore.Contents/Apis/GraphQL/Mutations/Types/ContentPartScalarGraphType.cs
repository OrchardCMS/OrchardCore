using GraphQL.Language.AST;
using GraphQL.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations.Types
{
    public class ContentPartScalarGraphType : ScalarGraphType
    {
        public ContentPartScalarGraphType()
        {
            Name = "ContentPart";
        }

        public override object ParseLiteral(IValue value)
        {
            return null;
        }

        public override object ParseValue(object value)
        {
            return null;
        }

        public override object Serialize(object value)
        {
            return null;
        }
    }
}
