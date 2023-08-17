using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;

namespace OrchardCore.DisplayManagement.Liquid.TagHelpers
{
    // Internal for testing
    internal static class RequiredAttributeParser
    {
        public static void AddRequiredAttributes(string requiredAttributes, TagMatchingRuleDescriptorBuilder ruleBuilder)
        {
            var requiredAttributeParser = new DefaultRequiredAttributeParser(requiredAttributes);
            requiredAttributeParser.AddRequiredAttributes(ruleBuilder);
        }

        private class DefaultRequiredAttributeParser
        {
            private const char RequiredAttributeWildcardSuffix = '*';

            private static readonly IReadOnlyDictionary<char, RequiredAttributeDescriptor.ValueComparisonMode> _cssValueComparisons =
                new Dictionary<char, RequiredAttributeDescriptor.ValueComparisonMode>
                {
                    { '=', RequiredAttributeDescriptor.ValueComparisonMode.FullMatch },
                    { '^', RequiredAttributeDescriptor.ValueComparisonMode.PrefixMatch },
                    { '$', RequiredAttributeDescriptor.ValueComparisonMode.SuffixMatch },
                };

            private static readonly char[] _invalidPlainAttributeNameCharacters = { ' ', '\t', ',', RequiredAttributeWildcardSuffix };

            private static readonly char[] _invalidCssAttributeNameCharacters = (new[] { ' ', '\t', ',', ']' })
                .Concat(_cssValueComparisons.Keys)
                .ToArray();

            private static readonly char[] _invalidCssQuotelessValueCharacters = { ' ', '\t', ']' };

            private int _index;
            private readonly string _requiredAttributes;

            public DefaultRequiredAttributeParser(string requiredAttributes)
            {
                _requiredAttributes = requiredAttributes;
            }

            private char Current => _requiredAttributes[_index];

            private bool AtEnd => _index >= _requiredAttributes.Length;

            public void AddRequiredAttributes(TagMatchingRuleDescriptorBuilder ruleBuilder)
            {
                if (String.IsNullOrEmpty(_requiredAttributes))
                {
                    return;
                }

                PassOptionalWhitespace();

                do
                {
                    var successfulParse = true;
                    ruleBuilder.Attribute(attributeBuilder =>
                    {
                        if (At('['))
                        {
                            if (!TryParseCssSelector(attributeBuilder))
                            {
                                successfulParse = false;
                                return;
                            }
                        }
                        else
                        {
                            ParsePlainSelector(attributeBuilder);
                        }

                        PassOptionalWhitespace();

                        if (At(','))
                        {
                            _index++;

                            if (!EnsureNotAtEnd(attributeBuilder))
                            {
                                successfulParse = false;
                                return;
                            }
                        }
                        else if (!AtEnd)
                        {
                            //var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeCharacter(Current, _requiredAttributes);
                            //attributeBuilder.Diagnostics.Add(diagnostic);
                            successfulParse = false;
                            return;
                        }

                        PassOptionalWhitespace();
                    });

                    if (!successfulParse)
                    {
                        break;
                    }
                }
                while (!AtEnd);
            }

            private void ParsePlainSelector(RequiredAttributeDescriptorBuilder attributeBuilder)
            {
                var nameEndIndex = _requiredAttributes.IndexOfAny(_invalidPlainAttributeNameCharacters, _index);
                string attributeName;

                var nameComparison = RequiredAttributeDescriptor.NameComparisonMode.FullMatch;
                if (nameEndIndex == -1)
                {
                    attributeName = _requiredAttributes[_index..];
                    _index = _requiredAttributes.Length;
                }
                else
                {
                    attributeName = _requiredAttributes[_index..nameEndIndex];
                    _index = nameEndIndex;

                    if (_requiredAttributes[nameEndIndex] == RequiredAttributeWildcardSuffix)
                    {
                        nameComparison = RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch;

                        // Move past wild card
                        _index++;
                    }
                }

                attributeBuilder.Name = attributeName;
                attributeBuilder.NameComparisonMode = nameComparison;
            }

            private void ParseCssAttributeName(RequiredAttributeDescriptorBuilder builder)
            {
                var nameStartIndex = _index;
                var nameEndIndex = _requiredAttributes.IndexOfAny(_invalidCssAttributeNameCharacters, _index);
                nameEndIndex = nameEndIndex == -1 ? _requiredAttributes.Length : nameEndIndex;
                _index = nameEndIndex;

                var attributeName = _requiredAttributes[nameStartIndex..nameEndIndex];

                builder.Name = attributeName;
            }

            private bool TryParseCssValueComparison(RequiredAttributeDescriptorBuilder builder, out RequiredAttributeDescriptor.ValueComparisonMode valueComparison)
            {
                Debug.Assert(!AtEnd);

                if (_cssValueComparisons.TryGetValue(Current, out valueComparison))
                {
                    var op = Current;
                    _index++;

                    if (op != '=' && At('='))
                    {
                        // Two length operator (ex: ^=). Move past the second piece
                        _index++;
                    }
                    else if (op != '=') // We're at an incomplete operator (ex: [foo^]
                    {
                        //var diagnostic = RazorDiagnosticFactory.CreateTagHelper_PartialRequiredAttributeOperator(op, _requiredAttributes);
                        //builder.Diagnostics.Add(diagnostic);

                        return false;
                    }
                }
                else if (!At(']'))
                {
                    //var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeOperator(Current, _requiredAttributes);
                    //builder.Diagnostics.Add(diagnostic);

                    return false;
                }

                builder.ValueComparisonMode = valueComparison;

                return true;
            }

            private bool TryParseCssValue(RequiredAttributeDescriptorBuilder builder)
            {
                int valueStart;
                int valueEnd;
                if (At('\'') || At('"'))
                {
                    var quote = Current;

                    // Move past the quote
                    _index++;

                    valueStart = _index;
                    valueEnd = _requiredAttributes.IndexOf(quote, _index);
                    if (valueEnd == -1)
                    {
                        //var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeMismatchedQuotes(quote, _requiredAttributes);
                        //builder.Diagnostics.Add(diagnostic);

                        return false;
                    }
                    _index = valueEnd + 1;
                }
                else
                {
                    valueStart = _index;
                    var valueEndIndex = _requiredAttributes.IndexOfAny(_invalidCssQuotelessValueCharacters, _index);
                    valueEnd = valueEndIndex == -1 ? _requiredAttributes.Length : valueEndIndex;
                    _index = valueEnd;
                }

                var value = _requiredAttributes[valueStart..valueEnd];

                builder.Value = value;

                return true;
            }

            private bool TryParseCssSelector(RequiredAttributeDescriptorBuilder attributeBuilder)
            {
                Debug.Assert(At('['));

                // Move past '['.
                _index++;
                PassOptionalWhitespace();

                ParseCssAttributeName(attributeBuilder);

                PassOptionalWhitespace();

                if (!EnsureNotAtEnd(attributeBuilder))
                {
                    return false;
                }

                if (!TryParseCssValueComparison(attributeBuilder, out var valueComparison))
                {
                    return false;
                }

                PassOptionalWhitespace();

                if (!EnsureNotAtEnd(attributeBuilder))
                {
                    return false;
                }

                if (valueComparison != RequiredAttributeDescriptor.ValueComparisonMode.None && !TryParseCssValue(attributeBuilder))
                {
                    return false;
                }

                PassOptionalWhitespace();

                if (At(']'))
                {
                    // Move past the ending bracket.
                    _index++;
                    return true;
                }
                else if (AtEnd)
                {
                    // var diagnostic = RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace(_requiredAttributes);
                    // attributeBuilder.Diagnostics.Add(diagnostic);
                }
                else
                {
                    // var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidRequiredAttributeCharacter(Current, _requiredAttributes);
                    // attributeBuilder.Diagnostics.Add(diagnostic);
                }

                return false;
            }

            private bool EnsureNotAtEnd(RequiredAttributeDescriptorBuilder _)
            {
                if (AtEnd)
                {
                    // var diagnostic = RazorDiagnosticFactory.CreateTagHelper_CouldNotFindMatchingEndBrace(_requiredAttributes);
                    // builder.Diagnostics.Add(diagnostic);

                    return false;
                }

                return true;
            }

            private bool At(char c)
            {
                return !AtEnd && Current == c;
            }

            private void PassOptionalWhitespace()
            {
                while (!AtEnd && (Current == ' ' || Current == '\t'))
                {
                    _index++;
                }
            }
        }
    }
}
