using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Orchard.Environment.Shell;
using Orchard.Parser.Yaml;
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
"  \"description\": \"A recipe providing only the core Orchard framework, with limited end-user functionality. This is useful for development scenarios.\"," +
"  \"author\": \"The Orchard Team\"," +
"  \"website\": \"http://orchardproject.net\"," +
"  \"version\": \"2.0\"," +
"  \"issetuprecipe\": true," +
"  \"category\": \"default\"," +
"  \"tags\": \"developer\"," +
"  \"steps\": [" +
"    {" +
"      \"name\": \"feature\"," +
"      \"disable\": [ ]," +
"      \"enable\": [" +
"        \"Orchard.Logging.Console\"," +
"        \"Orchard.Hosting\"," +
"        \"Settings\"," +
"        \"Dashboard\"," +
"        \"Navigation\"," +
"        \"Orchard.Themes\"," +
"        \"Orchard.Demo\"," +
"        \"Orchard.DynamicCache\"," +
"        \"TheTheme\"," +
"        \"TheAdmin\"," +
"        \"SafeMode\"" +
"      ]" +
"    }" +
"  ]" +
"}";
            File.WriteAllText(_tempFolderName, json);

            var fileInfo = new PhysicalFileInfo(new FileInfo(_tempFolderName));

            var parser = new RecipeParser();
            parser.ProcessRecipe(fileInfo, (recipe, step) =>
            {


            });
        }
    }
}