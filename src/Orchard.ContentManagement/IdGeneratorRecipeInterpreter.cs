using Orchard.Recipes.Services;

namespace Orchard.ContentManagement
{
    public class IdGeneratorRecipeInterpreter : IRecipeInterpreter
    {
        private readonly IContentItemIdGenerator _contentItemIdGenerator;

        public IdGeneratorRecipeInterpreter(IContentItemIdGenerator contentItemIdGenerator)
        {
            _contentItemIdGenerator = contentItemIdGenerator;
        }

        public bool TryEvaluate(string token, out string result)
        {
            if (token == "uuid()")
            {
                result = _contentItemIdGenerator.GenerateUniqueId(new ContentItem());
                return true;
            }

            result = null;
            return false;
        }
    }
}
