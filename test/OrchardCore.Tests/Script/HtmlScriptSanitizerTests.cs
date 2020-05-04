using Markdig;
using OrchardCore.Infrastructure.Script;
using Xunit;

namespace OrchardCore.Tests.Script
{
    public class HtmlScriptSanitizerTests
    {
        private static readonly HtmlScriptSanitizer _sanitizer = new HtmlScriptSanitizer();

        [Theory]
        [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
        [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
        [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
        public void ShouldSanitizeHTML(string html, string sanitized)
        {
            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.Equal(output, sanitized);
        }

        // Markdown samples from
        //https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet
        [Fact]
        public void MarkdownMatchesSanitized()
        {
            var markdown = @"# H1
                            ## H2
                            ### H3
                            #### H4
                            ##### H5
                            ###### H6

                            Alternatively, for H1 and H2, an underline-ish style:

                            Alt-H1
                            ======

                            Alt-H2
                            ------
                            Emphasis, aka italics, with *asterisks* or _underscores_.

                            Strong emphasis, aka bold, with **asterisks** or __underscores__.

                            Combined emphasis with **asterisks and _underscores_**.

                            Strikethrough uses two tildes. ~~Scratch this.~~

                            1. First ordered list item
                            2. Another item
                            ⋅⋅* Unordered sub-list. 
                            1. Actual numbers don't matter, just that it's a number
                            ⋅⋅1. Ordered sub-list
                            4. And another item.

                            ⋅⋅⋅You can have properly indented paragraphs within list items. Notice the blank line above, and the leading spaces (at least one, but we'll use three here to also align the raw Markdown).

                            ⋅⋅⋅To have a line break without a paragraph, you will need to use two trailing spaces.⋅⋅
                            ⋅⋅⋅Note that this line is separate, but within the same paragraph.⋅⋅
                            ⋅⋅⋅(This is contrary to the typical GFM line break behaviour, where trailing spaces are not required.)

                            * Unordered list can use asterisks
                            - Or minuses
                            + Or pluses

                            [You can use numbers for reference - style link definitions][1]

                            Or leave it empty and use the[link text itself].

                            URLs and URLs in angle brackets will automatically get turned into links.
                            http://www.example.com or <http://www.example.com> and sometimes 
                            example.com(but not on Github, for example).

                            Some text to show that the reference links can follow later.

                            [arbitrary case-insensitive reference text]: https://www.mozilla.org
                            [1]: http://slashdot.org
                            [link text itself]: http://www.reddit.com

                            Here's our logo (hover to see the title text):


                            Inline `code` has `back-ticks around` it.

                            Colons can be used to align columns.

                            | Tables        | Are           | Cool  |
                            | ------------- |:-------------:| -----:|
                            | col 3 is      | right-aligned | $1600 |
                            | col 2 is      | centered      |   $12 |
                            | zebra stripes | are neat      |    $1 |

                            There must be at least 3 dashes separating each header cell.
                            The outer pipes (|) are optional, and you don't need to make the 
                            raw Markdown line up prettily. You can also use inline Markdown.

                            Markdown | Less | Pretty
                            --- | --- | ---
                            *Still* | `renders` | **nicely**
                            1 | 2 | 3

                            > Blockquotes are very handy in email to emulate reply text.
                            > This line is part of the same quote.

                            Quote break.

                            > This is a very long line that will still be quoted properly when it wraps. Oh boy let's keep writing to make sure this is long enough to actually wrap for everyone. Oh, you can *put* **Markdown** into a blockquote. 
            ";

            var html = Markdig.Markdown.ToHtml(markdown);

            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.Equal(html, output);
        }

        [Fact]
        public void MarkdownImagesDoNotMatchSanitized()
        {
            // Does not match because Markdown produces an img tag " />" and sanitizer changes it to ">"
            var markdown = @"Inline-style: 
![alt text](https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png ""Logo Title Text 1"")
";

            var html = Markdig.Markdown.ToHtml(markdown);

            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.NotEqual(html, output);
        }

        [Fact]
        public void MarkdownWithSafeHtmlDoesNotMatchSanitized()
        {
            // Does not match because the sanitizer changes the entity escaping.
            var markdown = @"<a href=""foo"">bar</a>";

            var pipeline = new MarkdownPipelineBuilder()
                .DisableHtml()
                .Build();

            var html = Markdig.Markdown.ToHtml(markdown, pipeline);

            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.NotEqual(html, output);
        }

        [Fact]
        public void MarkdownWithCodeDoesNotMatchSanitizedHtml()
        {
            // Does not match because class hijacking is disabled by default in the sanitizer
            var markdown = @"```javascript
var s = ""JavaScript syntax highlighting"";
alert(s);
```";

            var pipeline = new MarkdownPipelineBuilder()
                .DisableHtml()
                .Build();

            var html = Markdig.Markdown.ToHtml(markdown, pipeline);

            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.NotEqual(html, output);
        }
    }
}
