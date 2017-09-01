using System.IO;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Localization.PortableObject;
using Xunit;

namespace Orchard.Tests.Localization
{
    public class PoParserTests
    {
        [Fact]
        public void ParseRetursSimpleEntry()
        {
            // msgid "Unknown system error"
            // msgstr "Error desconegut del sistema"
            var entries = ParseText("SimpleEntry");

            Assert.Equal("Unknown system error", entries[0].Key);
            Assert.Equal("Error desconegut del sistema", entries[0].Translations[0]);
        }

        [Fact]
        public void ParseIgnoresEntryWithoutTranslation()
        {
            // "msgid "Unknown system error"
            // "msgstr ""
            var entries = ParseText("EntryWithoutTranslation");

            Assert.Empty(entries);
        }

        [Fact]
        public void ParseIgnoresPoeditHeader()
        {
            // msgid ""
            // msgstr ""
            // "POT-Creation-Date: 2016-06-22 17:06+0200\n"
            // "PO-Revision-Date: 2016-08-01 11:57+0200\n"
            // "X-Poedit-Basepath: ../../..\n"
            var entries = ParseText("PoeditHeader");

            Assert.Empty(entries);
        }

        [Fact]
        public void ParseSetsContext()
        {
            // msgctxt "Orchard.Localization"
            // msgid "Unknown system error"
            // msgstr "Error desconegut del sistema"
            var entries = ParseText("EntryWithContext");

            Assert.Equal("Orchard.Localization|Unknown system error", entries[0].Key, ignoreCase: true);
        }

        [Fact]
        public void ParseIgnoresComments()
        {
            // # translator-comments
            // #. extracted-comments
            // #: reference…
            // #, flag

            // #| msgctxt previous-context
            // #| msgid previous-untranslated-string
            // msgctxt "Orchard.Localization"
            // msgid "Unknown system error"
            // msgstr "Error desconegut del sistema"

            var entries = ParseText("EntryWithComments");

            Assert.Equal("Orchard.Localization|Unknown system error", entries[0].Key, ignoreCase: true);
            Assert.Equal("Error desconegut del sistema", entries[0].Translations[0]);
        }

        [Fact]
        public void ParseOnlyTrimsLeadingAndTrailingQuotes()
        {
            // msgid "\"{0}\""
            // msgstr "\"{0}\""

            var entries = ParseText("EntryWithQuotes");

            Assert.Equal("\"{0}\"", entries[0].Key);
            Assert.Equal("\"{0}\"", entries[0].Translations[0]);
        }

        [Fact]
        public void ParseHandleUnclosedQuote()
        {
            // msgctxt "
            // msgid "Foo \"{0}\""
            // msgstr "Foo \"{0}\""

            var entries = ParseText("EntryWithUnclosedQuote");

            Assert.Equal("Foo \"{0}\"", entries[0].Key);
        }


        [Fact]
        public void ParseHandlesMultilineEntry()
        {
            // msgid ""
            // "Here is an example of how one might continue a very long string\n"
            // "for the common case the string represents multi-line output."
            // msgstr ""
            // "Here is an example of how one might continue a very long translation\n"
            // "for the common case the string represents multi-line output."

            var entries = ParseText("EntryWithMultilineText");

            Assert.Equal("Here is an example of how one might continue a very long string\nfor the common case the string represents multi-line output.", entries[0].Key);
            Assert.Equal("Here is an example of how one might continue a very long translation\nfor the common case the string represents multi-line output.", entries[0].Translations[0]);
        }

        [Fact]
        public void ParsePreservesEscapedCharacters()
        {
            // msgid "Line:\t\"{0}\"\n"
            // msgstr "Line:\t\"{0}\"\n"

            var entries = ParseText("EntryWithEscapedCharacters");

            Assert.Equal("Line:\t\"{0}\"\n", entries[0].Key);
            Assert.Equal("Line:\t\"{0}\"\n", entries[0].Translations[0]);
        }

        [Fact]
        public void ParseReadsPluralTranslations()
        {
            // msgid "book"
            // msgid_plural "books"
            // msgstr[0] "kniha"
            // msgstr[1] "knihy"
            // msgstr[2] "knih"

            var entries = ParseText("EntryWithPlural");

            Assert.Equal("book", entries[0].Key);
            Assert.Equal("kniha", entries[0].Translations[0]);
            Assert.Equal("knihy", entries[0].Translations[1]);
            Assert.Equal("knih", entries[0].Translations[2]);
        }

        [Fact]
        public void ParseReadsMultipleEntries()
        {
            // #. "File {0} does not exist"
            // msgctxt "Orchard.FileSystems.Media.FileSystemStorageProvider"
            // msgid "File {0} does not exist"
            // msgstr "Soubor {0} neexistuje"

            // #. "Directory {0} does not exist"
            // msgctxt "Orchard.FileSystems.Media.Directory"
            // msgid "Directory {0} does not exist"
            // msgstr "Složka {0} neexistuje"

            var entries = ParseText("MultipleEntries");

            Assert.Equal(2, entries.Length);

            Assert.Equal("Orchard.FileSystems.Media.FileSystemStorageProvider|File {0} does not exist", entries[0].Key, ignoreCase: true);
            Assert.Equal("Soubor {0} neexistuje", entries[0].Translations[0]);

            Assert.Equal("Orchard.FileSystems.Media.Directory|Directory {0} does not exist", entries[1].Key, ignoreCase: true);
            Assert.Equal("Složka {0} neexistuje", entries[1].Translations[0]);
        }

        private CultureDictionaryRecord[] ParseText(string resourceName)
        {
            var parser = new PoParser();

            var testAssembly = typeof(PoParserTests).GetTypeInfo().Assembly;
            using (var resource = testAssembly.GetManifestResourceStream("Orchard.Tests.Localization.PoFiles." + resourceName + ".po"))
            {
                using (var reader = new StreamReader(resource))
                {
                    return parser.Parse(reader).ToArray();
                }
            }
        }
    }
}
