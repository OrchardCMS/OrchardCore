using Microsoft.Extensions.FileProviders.Physical;
using Orchard.Recipes.Services;
using System;
using System.IO;
using Xunit;

namespace Orchard.Tests.Configuration
{
    public class RecipeParserTests : IDisposable
    {
        private string _tempFolderName;

        public RecipeParserTests()
        {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
        }

        public void Dispose()
        {
            File.Delete(_tempFolderName);
        }

        [Fact]
        public void Foo()
        {
            var json = @"{" +
"  \"name\": \"core\"," +
"  \"description\": \"core descriptor.\"," +
"  \"author\": \"The Orchard Team\"," +
"  \"website\": \"http://orchardproject.net\"," +
"  \"version\": \"2.0\"," +
"  \"issetuprecipe\": true," +
"  \"categories\": [ \"default\" ]," +
"  \"tags\": [ \"developer\" ]," +
"  \"steps\": [" +
"    {" +
"      \"name\": \"feature\"," +
"      \"disable\": [ ]," +
"      \"enable\": [" +
"        \"Orchard.Logging.Console\"," +
"        \"Orchard.Hosting\"" +
"      ]" +
"    }" +
"  ]" +
"}";
            File.WriteAllText(_tempFolderName, json);

            var fileInfo = new PhysicalFileInfo(new FileInfo(_tempFolderName));

            var parser = new JsonRecipeParser();
            var descriptor = parser.ParseRecipe(fileInfo);
            Assert.Equal("core", descriptor.Name);
            Assert.Equal("core descriptor.", descriptor.Description);
            Assert.Equal("The Orchard Team", descriptor.Author);
            Assert.Equal("http://orchardproject.net", descriptor.WebSite);
            Assert.Equal("2.0", descriptor.Version);
            Assert.Equal(true, descriptor.IsSetupRecipe);
            Assert.Equal("default", descriptor.Categories[0]);
            Assert.Equal("developer", descriptor.Tags[0]);
        }
    }
}