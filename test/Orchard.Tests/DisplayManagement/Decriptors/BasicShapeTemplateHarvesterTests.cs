using System.Linq;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Xunit;

namespace Orchard.Tests.DisplayManagement.Decriptors
{
    public class BasicShapeTemplateHarvesterTests
    {
        private static void VerifyShapeType(string givenSubPath, string givenFileName, string expectedShapeType)
        {
            var harvester = new BasicShapeTemplateHarvester();
            var harvestShapeHits = harvester.HarvestShape(new HarvestShapeInfo { SubPath = givenSubPath, FileName = givenFileName });
            Assert.Equal(1, harvestShapeHits.Count());
            Assert.Equal(expectedShapeType, harvestShapeHits.Single().ShapeType, ignoreCase: true);
        }

        [Fact]
        public void BasicFileNamesComeBackAsShapes()
        {
            VerifyShapeType("Views/Shared/Shapes", "Hello", "Hello");
            VerifyShapeType("Views/Shared/Shapes", "World", "World");
        }

        [Fact]
        public void DashBecomesBreakingSeperator()
        {
            VerifyShapeType("Views/Shared/Shapes", "Hello-World", "Hello__World");
        }

        [Fact]
        public void DotBecomesNonBreakingSeperator()
        {
            VerifyShapeType("Views/Shared/Shapes", "Hello.World", "Hello_World");
        }

        [Fact]
        public void DefaultItemsContentTemplate()
        {
            VerifyShapeType("Views/Shared/Shapes/Items", "Content", "Content");
        }

        [Fact]
        public void ImplicitSpecializationOfItemsContentTemplate()
        {
            VerifyShapeType("Views/Shared/Shapes/Items", "MyType", "MyType");
        }

        [Fact]
        public void ExplicitSpecializationOfItemsContentTemplate()
        {
            VerifyShapeType("Views/Shared/Shapes/Items", "Content-MyType", "Content__MyType");
        }

        [Fact]
        public void ContentItemDisplayTypes()
        {
            VerifyShapeType("Views/Shared/Shapes/Items", "Content", "Content");
            VerifyShapeType("Views/Shared/Shapes/Items", "Content.Summary", "Content_Summary");
            VerifyShapeType("Views/Shared/Shapes/Items", "Content.Edit", "Content_Edit");
        }

        [Fact]
        public void ExplicitSpecializationMixedWithDisplayTypes()
        {
            VerifyShapeType("Views/Shared/Shapes/Items", "Content-MyType", "Content__MyType");
            VerifyShapeType("Views/Shared/Shapes/Items", "Content-MyType.Summary", "Content_Summary__MyType");
            VerifyShapeType("Views/Shared/Shapes/Items", "Content-MyType.Edit", "Content_Edit__MyType");
        }

        [Fact]
        public void DefaultItemsContentTemplate2()
        {
            VerifyShapeType("Views/Shared/Shapes", "Content", "Content");
        }

        [Fact]
        public void ImplicitSpecializationOfItemsContentTemplate2()
        {
            VerifyShapeType("Views/Shared/Shapes", "MyType", "MyType");
        }

        [Fact]
        public void ExplicitSpecializationOfItemsContentTemplate2()
        {
            VerifyShapeType("Views/Shared/Shapes", "Content-MyType", "Content__MyType");
        }

        [Fact]
        public void ContentItemDisplayTypes2()
        {
            VerifyShapeType("Views/Shared/Shapes", "Content", "Content");
            VerifyShapeType("Views/Shared/Shapes", "Content.Summary", "Content_Summary");
            VerifyShapeType("Views/Shared/Shapes", "Content.Edit", "Content_Edit");
        }

        [Fact]
        public void ExplicitSpecializationMixedWithDisplayTypes2()
        {
            VerifyShapeType("Views/Shared/Shapes", "Content-MyType", "Content__MyType");
            VerifyShapeType("Views/Shared/Shapes", "Content-MyType.Summary", "Content_Summary__MyType");
            VerifyShapeType("Views/Shared/Shapes", "Content-MyType.Edit", "Content_Edit__MyType");
        }

        [Fact]
        public void MultipleDotsAreNormalizedToUnderscore()
        {
            VerifyShapeType("Views/Shared/Shapes/Parts", "Common.Body", "Parts_Common_Body");
            VerifyShapeType("Views/Shared/Shapes/Parts", "Common.Body.Summary", "Parts_Common_Body_Summary");
            VerifyShapeType("Views/Shared/Shapes/Parts", "Localization.ContentTranslations.Summary", "Parts_Localization_ContentTranslations_Summary");
        }

        [Fact]
        public void MultipleDotsAreNormalizedToUnderscore2()
        {
            VerifyShapeType("Views/Shared/Shapes", "Parts.Common.Body", "Parts_Common_Body");
            VerifyShapeType("Views/Shared/Shapes", "Parts.Common.Body.Summary", "Parts_Common_Body_Summary");
            VerifyShapeType("Views/Shared/Shapes", "Parts.Localization.ContentTranslations.Summary", "Parts_Localization_ContentTranslations_Summary");
        }

        [Fact]
        public void FieldNamesMayBeInSubfolderOrPrefixed()
        {
            VerifyShapeType("Views/Shared/Shapes/Fields", "Common.Text", "Fields_Common_Text");
            VerifyShapeType("Views/Shared/Shapes", "Fields.Common.Text", "Fields_Common_Text");
        }

        [Fact]
        public void FieldNamesMayHaveLongOrShortAlternates()
        {
            VerifyShapeType("Views/Shared/Shapes/Fields", "Common.Text-FirstName", "Fields_Common_Text__FirstName");
            VerifyShapeType("Views/Shared/Shapes/Fields", "Common.Text-FirstName.SpecialCase", "Fields_Common_Text_SpecialCase__FirstName");
            VerifyShapeType("Views/Shared/Shapes/Fields", "Common.Text-FirstName-MyContentType", "Fields_Common_Text__FirstName__MyContentType");

            VerifyShapeType("Views/Shared/Shapes", "Fields.Common.Text-FirstName", "Fields_Common_Text__FirstName");
            VerifyShapeType("Views/Shared/Shapes", "Fields.Common.Text-FirstName.SpecialCase", "Fields_Common_Text_SpecialCase__FirstName");
            VerifyShapeType("Views/Shared/Shapes", "Fields.Common.Text-FirstName-MyContentType", "Fields_Common_Text__FirstName__MyContentType");
        }
    }
}