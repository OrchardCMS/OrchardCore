using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Util.Automaton;
using Newtonsoft.Json.Linq;
using Orchard.Lucene;

namespace Orchard.Queries
{
    public class QueryDslBuilder
    {
        private readonly Analyzer _analyzer;

        public QueryDslBuilder(Analyzer analyzer)
        {
            _analyzer = analyzer;
        }

        public Query BuildQuery(JObject query)
        {
            var first = query.Properties().First();

            switch (first.Name.ToLowerInvariant())
            {
                case "query":
                    return CreateQuery((JObject)first.Value);

                // Term-level queries
                case "term":
                    return CreateTermQuery((JObject)first.Value);
                case "terms":
                    return CreateTermsQuery((JObject)first.Value);
                case "range":
                    return CreateRangeQuery((JObject)first.Value);
                case "prefix":
                    return CreatePrefixQuery((JObject)first.Value);
                case "wildcard":
                    return CreateWildcardQuery((JObject)first.Value);
                case "regexp":
                    return CreateRegexpQuery((JObject)first.Value);
                case "fuzzy":
                    return CreateFuzzyQuery((JObject)first.Value);

                // Full-text queries
                case "match":
                    return CreateMatchQuery((JObject)first.Value);
                case "match_phrase":
                    return CreateMatchPhraseQuery((JObject)first.Value);
                case "query_string":
                    return CreateQueryStringQuery((JObject)first.Value);
                case "simple_query_string":
                    return CreateSimpleQueryStringQuery((JObject)first.Value);
                
                // Compound queries
                case "bool":
                    return CreateBoolQuery((JObject)first.Value);

                default:
                    throw new ArgumentException("Invalid query");
            }
        }

        public Query CreateQuery(JObject query)
        {
            return BuildQuery(query);
        }

        public TermQuery CreateTermQuery(JObject query)
        {
            var first = query.Properties().First();

            // A term query has only one member, which can either be a string or an object

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new TermQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var value = obj.Property("value").Value.Value<string>();
                    var termQuery = new TermQuery(new Term(first.Name, value));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        termQuery.Boost = boost.Value<float>();
                    }

                    return termQuery;
                default: throw new ArgumentException("Invalid term query");
            }
        }

        public Query CreateTermsQuery(JObject query)
        {
            var first = query.Properties().First();

            string field = first.Name;
            var boolQuery = new BooleanQuery();

            switch (first.Value.Type)
            {
                case JTokenType.Array:

                    foreach (var item in ((JArray)first.Value))
                    {
                        if (item.Type != JTokenType.String)
                        {
                            throw new ArgumentException($"Invalid term in terms query");
                        }

                        boolQuery.Add(new TermQuery(new Term(field, item.Value<string>())), BooleanClause.Occur.SHOULD);
                    }

                    break;
                case JTokenType.Object:
                    throw new ArgumentException("The terms lookup query is not supported");
                default: throw new ArgumentException("Invalid terms query");
            }

            return boolQuery;
        }

        public Query CreateFuzzyQuery(JObject query)
        {
            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new FuzzyQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in fuzzy query");
                    }

                    obj.TryGetValue("fuzziness", out var fuzziness);
                    obj.TryGetValue("prefix_length", out var prefixLength);
                    obj.TryGetValue("max_expansions", out var maxExpansions);

                    var fuzzyQuery = new FuzzyQuery(
                        new Term(first.Name, value.Value<string>()), 
                        fuzziness?.Value<int>() ?? LevenshteinAutomata.MAXIMUM_SUPPORTED_DISTANCE,
                        prefixLength?.Value<int>() ?? 0,
                        maxExpansions?.Value<int>() ?? 50,
                        true);

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        fuzzyQuery.Boost = boost.Value<float>();
                    }

                    return fuzzyQuery;
                default: throw new ArgumentException("Invalid fuzzy query");
            }
        }

        public Query CreateWildcardQuery(JObject query)
        {
            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new WildcardQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in wildcard query");
                    }

                    var wildCardQuery = new WildcardQuery(new Term(value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        wildCardQuery.Boost = boost.Value<float>();
                    }

                    return wildCardQuery;
                default: throw new ArgumentException("Invalid wildcard query");
            }
        }

        public Query CreateRegexpQuery(JObject query)
        {
            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new RegexpQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in regexp query");
                    }

                    // TODO: Support flags

                    var regexpQuery = new RegexpQuery(new Term(value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        regexpQuery.Boost = boost.Value<float>();
                    }

                    return regexpQuery;
                default: throw new ArgumentException("Invalid regexp query");
            }
        }

        public Query CreateMatchPhraseQuery(JObject query)
        {
            var first = query.Properties().First();

            var phraseQuery = new PhraseQuery();
            JToken value;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    value = first.Value;
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out value))
                    {
                        throw new ArgumentException("Missing value in match phrase query");
                    }

                    // TODO: read "analyzer" property

                    if (obj.TryGetValue("slop", out var slop))
                    {
                        phraseQuery.Slop = slop.Value<int>();
                    }

                    break;
                default: throw new ArgumentException("Invalid wildcard query");
            }

            foreach (var term in AnalyzeText(first.Name, value.Value<string>()))
            {
                phraseQuery.Add(new Term(first.Name, term));
            }

            return phraseQuery;
        }

        public PrefixQuery CreatePrefixQuery(JObject query)
        {
            var first = query.Properties().First();

            // A prefix query has only one member, which can either be a string or an object

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new PrefixQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    PrefixQuery prefixQuery = null;

                    if (obj.TryGetValue("value", out var value))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, value.Value<string>()));
                    }
                    else if (obj.TryGetValue("prefix", out var prefix))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, prefix.Value<string>()));
                    }
                    else
                    {
                        throw new ArgumentException("Prefix query misses prefix value");
                    }

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        prefixQuery.Boost = boost.Value<float>();
                    }

                    return prefixQuery;
                default: throw new ArgumentException("Invalid prefix query");
            }
        }
        public Query CreateRangeQuery(JObject query)
        {
            var range = query.Properties().First();
            Query rangeQuery;

            switch (range.Value.Type)
            {
                case JTokenType.Object:
                    var field = range.Name;

                    JToken gt = null;
                    JToken lt = null;
                    JTokenType type = JTokenType.None;
                    float? boost = null;

                    bool includeLower = false, includeUpper = false;

                    foreach(var element in ((JObject)range.Value).Properties())
                    {
                        switch (element.Name.ToLowerInvariant())
                        {
                            case "gt":
                                gt = element.Value;
                                type = gt.Type;
                                break;
                            case "gte":
                                gt = element.Value;
                                type = gt.Type;
                                includeLower = true;
                                break;
                            case "lt":
                                lt = element.Value;
                                type = lt.Type;
                                break;
                            case "lte":
                                lt = element.Value;
                                type = lt.Type;
                                includeUpper = true;
                                break;
                            case "boost":
                                boost = element.Value.Value<float>();
                                break;
                        }
                    }

                    if (gt != null && lt != null && gt.Type != lt.Type)
                    {
                        throw new ArgumentException("Lower and upper bound range types don't match");
                    }

                    switch (type)
                    {
                        case JTokenType.Integer:
                            var minInt = gt?.Value<int>();
                            var maxInt = lt?.Value<int>();
                            rangeQuery = NumericRangeQuery.NewIntRange(field, minInt, maxInt, includeLower, includeUpper);
                            break;
                        case JTokenType.Float:
                            var minFloat = gt?.Value<double>();
                            var maxFloat = lt?.Value<double>();
                            rangeQuery = NumericRangeQuery.NewDoubleRange(field, minFloat, maxFloat, includeLower, includeUpper);
                            break;
                        case JTokenType.String:
                            var minString = gt?.Value<string>();
                            var maxString = lt?.Value<string>();
                            rangeQuery = TermRangeQuery.NewStringRange(field, minString, maxString, includeLower, includeUpper);
                            break;
                        default: throw new ArgumentException($"Unsupported range value type: {type}");
                    }

                    if (boost != null)
                    {
                        rangeQuery.Boost = boost.Value;
                    }

                    return rangeQuery;
                default: throw new ArgumentException("Invalid range query");
            }
        }

        public Query CreateMatchQuery(JObject query)
        {
            var first = query.Properties().First();

            var boolQuery = new BooleanQuery();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    foreach (var term in AnalyzeText(first.Name, first.Value.ToString()))
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), BooleanClause.Occur.SHOULD);
                    }
                    return boolQuery;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var value = obj.Property("query")?.Value.Value<string>();

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        boolQuery.Boost = boost.Value<float>();
                    }

                    var occur = BooleanClause.Occur.SHOULD;
                    if (obj.TryGetValue("operator", out var op))
                    {
                        occur = op.ToString() == "and" ? BooleanClause.Occur.MUST : BooleanClause.Occur.SHOULD;
                    }

                    var terms = AnalyzeText(first.Name, value);

                    if (!terms.Any())
                    {
                        if (obj.TryGetValue("zero_terms_query", out var zeroTermsQuery))
                        {
                            if (zeroTermsQuery.ToString() == "all")
                            {
                                return new MatchAllDocsQuery();
                            }
                        }
                    }

                    foreach (var term in terms)
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), occur);
                    }

                    return boolQuery;
                default: throw new ArgumentException("Invalid query");
            }
        }

        public BooleanQuery CreateBoolQuery(JObject query)
        {
            var boolQuery = new BooleanQuery();

            foreach(var property in query.Properties())
            {
                var occur = BooleanClause.Occur.MUST;

                switch (property.Name.ToLowerInvariant())
                {
                    case "must":
                        occur = BooleanClause.Occur.MUST;
                        break;
                    case "mustnot":
                    case "must_not":
                        occur = BooleanClause.Occur.MUST_NOT;
                        break;
                    case "should":
                        occur = BooleanClause.Occur.SHOULD;
                        break;
                    case "boost":
                        boolQuery.Boost = query.Value<float>();
                        break;
                    case "minimum_should_match":
                        boolQuery.MinimumNumberShouldMatch = query.Value<int>();
                        break;
                    default: throw new ArgumentException($"Invalid property '{property.Name}' in boolean query");
                }

                switch(property.Value.Type)
                {
                    case JTokenType.Object:
                        
                        break;
                    case JTokenType.Array:
                        foreach (var item in ((JArray)property.Value))
                        {
                            if (item.Type != JTokenType.Object)
                            {
                                throw new ArgumentException($"Invalid value in boolean query");
                            }
                            boolQuery.Add(CreateQuery((JObject)item), occur);
                        }
                        break;
                    default: throw new ArgumentException($"Invalid value in boolean query");

                }
            }

            return boolQuery;
        }
        
        public Query CreateQueryStringQuery(JObject queryString)
        {
            var query = queryString["query"]?.Value<string>();
            var queryParser = new QueryParser(LuceneSettings.DefaultVersion, "", _analyzer);
            return queryParser.Parse(query);
        }

        public Query CreateSimpleQueryStringQuery(JObject queryString)
        {
            var query = queryString["query"]?.Value<string>();
            var fields = queryString["fields"]?.Value<string>() ?? "";
            var defaultOperator = queryString["default_operator"]?.Value<string>() ?? "and";
            var queryParser = new SimpleQueryParser(_analyzer, "");
            return queryParser.Parse(query);
        }
        
        private List<string> AnalyzeText(string fieldName, string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            var result = new List<string>();
            using (var sr = new StringReader(text))
            {
                using (var tokenStream = _analyzer.TokenStream(fieldName, sr))
                {
                    tokenStream.Reset();
                    while (tokenStream.IncrementToken())
                    {
                        var termAttribute = tokenStream.GetAttribute<ICharTermAttribute>();

                        if (termAttribute != null)
                        {
                            result.Add(termAttribute.ToString());
                        }
                    }
                }
            }

            return result;
        }
    }
}
