using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;

namespace OrchardCore.Tests.DisplayManagement.Decriptors
{
    public class BasicShapeTemplateHarvesterTests
    {
        private static void VerifyShapeType(string givenSubPath, string givenFileName, string expectedShapeType)
        {
            var harvester = new BasicShapeTemplateHarvester();
            var harvestShapeHits = harvester.HarvestShape(new HarvestShapeInfo { SubPath = givenSubPath, FileName = givenFileName });
            Assert.Single(harvestShapeHits);
            Assert.Equal(expectedShapeType, harvestShapeHits.Single().ShapeType, ignoreCase: true);
        }

        [Fact]
        public void BasicFileNamesComeBackAsShapes()
        {
            VerifyShapeType("Views", "Hello", "Hello");
            VerifyShapeType("Views", "World", "World");
        }

        [Fact]
        public void DashBecomesBreakingSeperator()
        {
            VerifyShapeType("Views", "Hello-World", "Hello__World");
        }

        [Fact]
        public void DotBecomesNonBreakingSeperator()
        {
            VerifyShapeType("Views", "Hello.World", "Hello_World");
        }

        [Fact]
        public void DefaultItemsContentTemplate()
        {
            VerifyShapeType("Views/Items", "Content", "Content");
        }

        [Fact]
        public void ImplicitSpecializationOfItemsContentTemplate()
        {
            VerifyShapeType("Views/Items", "MyType", "MyType");
        }

        [Fact]
        public void ExplicitSpecializationOfItemsContentTemplate()
        {
            VerifyShapeType("Views/Items", "Content-MyType", "Content__MyType");
        }

        [Fact]
        public void ContentItemDisplayTypes()
        {
            VerifyShapeType("Views/Items", "Content", "Content");
            VerifyShapeType("Views/Items", "Content.Summary", "Content_Summary");
            VerifyShapeType("Views/Items", "Content.Edit", "Content_Edit");
        }

        [Fact]
        public void ExplicitSpecializationMixedWithDisplayTypes()
        {
            VerifyShapeType("Views/Items", "Content-MyType", "Content__MyType");
            VerifyShapeType("Views/Items", "Content-MyType.Summary", "Content_Summary__MyType");
            VerifyShapeType("Views/Items", "Content-MyType.Edit", "Content_Edit__MyType");
        }

        [Fact]
        public void DefaultItemsContentTemplate2()
        {
            VerifyShapeType("Views", "Content", "Content");
        }

        [Fact]
        public void ImplicitSpecializationOfItemsContentTemplate2()
        {
            VerifyShapeType("Views", "MyType", "MyType");
        }

        [Fact]
        public void ExplicitSpecializationOfItemsContentTemplate2()
        {
            VerifyShapeType("Views", "Content-MyType", "Content__MyType");
        }

        [Fact]
        public void ContentItemDisplayTypes2()
        {
            VerifyShapeType("Views", "Content", "Content");
            VerifyShapeType("Views", "Content.Summary", "Content_Summary");
            VerifyShapeType("Views", "Content.Edit", "Content_Edit");
        }

        [Fact]
        public void ExplicitSpecializationMixedWithDisplayTypes2()
        {
            VerifyShapeType("Views", "Content-MyType", "Content__MyType");
            VerifyShapeType("Views", "Content-MyType.Summary", "Content_Summary__MyType");
            VerifyShapeType("Views", "Content-MyType.Edit", "Content_Edit__MyType");
        }

        [Fact]
        public void MultipleDotsAreNormalizedToUnderscore()
        {
            VerifyShapeType("Views/Parts", "Common.Body", "Parts_Common_Body");
            VerifyShapeType("Views/Parts", "Common.Body.Summary", "Parts_Common_Body_Summary");
            VerifyShapeType("Views/Parts", "Localization.ContentTranslations.Summary", "Parts_Localization_ContentTranslations_Summary");
        }

        [Fact]
        public void MultipleDotsAreNormalizedToUnderscore2()
        {
            VerifyShapeType("Views", "Parts.Common.Body", "Parts_Common_Body");
            VerifyShapeType("Views", "Parts.Common.Body.Summary", "Parts_Common_Body_Summary");
            VerifyShapeType("Views", "Parts.Localization.ContentTranslations.Summary", "Parts_Localization_ContentTranslations_Summary");
        }

        [Fact]
        public void FieldNamesMayBeInSubfolderOrPrefixed()
        {
            VerifyShapeType("Views/Fields", "Common.Text", "Fields_Common_Text");
            VerifyShapeType("Views", "Fields.Common.Text", "Fields_Common_Text");
        }

        [Fact]
        public void FieldNamesMayHaveLongOrShortAlternates()
        {
            VerifyShapeType("Views/Fields", "Common.Text-FirstName", "Fields_Common_Text__FirstName");
            VerifyShapeType("Views/Fields", "Common.Text-FirstName.SpecialCase", "Fields_Common_Text_SpecialCase__FirstName");
            VerifyShapeType("Views/Fields", "Common.Text-FirstName-MyContentType", "Fields_Common_Text__FirstName__MyContentType");

            VerifyShapeType("Views", "Fields.Common.Text-FirstName", "Fields_Common_Text__FirstName");
            VerifyShapeType("Views", "Fields.Common.Text-FirstName.SpecialCase", "Fields_Common_Text_SpecialCase__FirstName");
            VerifyShapeType("Views", "Fields.Common.Text-FirstName-MyContentType", "Fields_Common_Text__FirstName__MyContentType");
        }
    }
}
