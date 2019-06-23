using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Support;
using Lucene.Net.Util;
using System;
using System.IO;
using System.Text;

namespace OrchardCore.Lucene.FrenchAnalyzer.Analyzers
{
    /*
	 * Licensed to the Apache Software Foundation (ASF) under one or more
	 * contributor license agreements.  See the NOTICE file distributed with
	 * this work for additional information regarding copyright ownership.
	 * The ASF licenses this file to You under the Apache License, Version 2.0
	 * (the "License"); you may not use this file except in compliance with
	 * the License.  You may obtain a copy of the License at
	 *
	 *     http://www.apache.org/licenses/LICENSE-2.0
	 *
	 * Unless required by applicable law or agreed to in writing, software
	 * distributed under the License is distributed on an "AS IS" BASIS,
	 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	 * See the License for the specific language governing permissions and
	 * limitations under the License.
	 */

    /// <summary>
    /// <see cref="Analyzer"/> for French language. 
    /// <para>
    /// Supports an external list of stopwords (words that
    /// will not be indexed at all) and an external list of exclusions (word that will
    /// not be stemmed, but indexed).
    /// A default set of stopwords is used unless an alternative list is specified, but the
    /// exclusion list is empty by default.
    /// </para>
    /// <para>You must specify the required <see cref="LuceneVersion"/>
    /// compatibility when creating FrenchAnalyzer:
    /// <list type="bullet">
    ///   <item><description> As of 3.6, <see cref="FrenchLightStemFilter"/> is used for less aggressive stemming.</description></item>
    ///   <item><description> As of 3.1, Snowball stemming is done with <see cref="SnowballFilter"/>, 
    ///        <see cref="LowerCaseFilter"/> is used prior to <see cref="StopFilter"/>, and <see cref="ElisionFilter"/> and 
    ///        Snowball stopwords are used by default.</description></item>
    ///   <item><description> As of 2.9, <see cref="StopFilter"/> preserves position
    ///        increments</description></item>
    /// </list>
    /// 
    /// </para>
    /// <para><b>NOTE</b>: This class uses the same <see cref="LuceneVersion"/>
    /// dependent settings as <see cref="StandardAnalyzer"/>.</para>
    /// </summary>
    public sealed class FrenchAnalyzer : StopwordAnalyzerBase
    {
        /// <summary>
        /// Extended list of typical French stopwords. </summary>
        /// @deprecated (3.1) remove in Lucene 5.0 (index bw compat) 
        [Obsolete("(3.1) remove in Lucene 5.0 (index bw compat)")]
        private static readonly string[] FRENCH_STOP_WORDS = new string[] {
            "a", "afin", "ai", "ainsi", "après", "attendu", "au", "aujourd", "auquel", "aussi",
            "autre", "autres", "aux", "auxquelles", "auxquels", "avait", "avant", "avec", "avoir",
            "c", "car", "ce", "ceci", "cela", "celle", "celles", "celui", "cependant", "certain",
            "certaine", "certaines", "certains", "ces", "cet", "cette", "ceux", "chez", "ci",
            "combien", "comme", "comment", "concernant", "contre", "d", "dans", "de", "debout",
            "dedans", "dehors", "delà", "depuis", "derrière", "des", "désormais", "desquelles",
            "desquels", "dessous", "dessus", "devant", "devers", "devra", "divers", "diverse",
            "diverses", "doit", "donc", "dont", "du", "duquel", "durant", "dès", "elle", "elles",
            "en", "entre", "environ", "est", "et", "etc", "etre", "eu", "eux", "excepté", "hormis",
            "hors", "hélas", "hui", "il", "ils", "j", "je", "jusqu", "jusque", "l", "la", "laquelle",
            "le", "lequel", "les", "lesquelles", "lesquels", "leur", "leurs", "lorsque", "lui", "là",
            "ma", "mais", "malgré", "me", "merci", "mes", "mien", "mienne", "miennes", "miens", "moi",
            "moins", "mon", "moyennant", "même", "mêmes", "n", "ne", "ni", "non", "nos", "notre",
            "nous", "néanmoins", "nôtre", "nôtres", "on", "ont", "ou", "outre", "où", "par", "parmi",
            "partant", "pas", "passé", "pendant", "plein", "plus", "plusieurs", "pour", "pourquoi",
            "proche", "près", "puisque", "qu", "quand", "que", "quel", "quelle", "quelles", "quels",
            "qui", "quoi", "quoique", "revoici", "revoilà", "s", "sa", "sans", "sauf", "se", "selon",
            "seront", "ses", "si", "sien", "sienne", "siennes", "siens", "sinon", "soi", "soit",
            "son", "sont", "sous", "suivant", "sur", "ta", "te", "tes", "tien", "tienne", "tiennes",
            "tiens", "toi", "ton", "tous", "tout", "toute", "toutes", "tu", "un", "une", "va", "vers",
            "voici", "voilà", "vos", "votre", "vous", "vu", "vôtre", "vôtres", "y", "à", "ça", "ès",
            "été", "être", "ô"
        };

        /// <summary>
        /// File containing default French stopwords. </summary>
        public const string DEFAULT_STOPWORD_FILE = "french_stop.txt";

        /// <summary>
        /// Default set of articles for <see cref="ElisionFilter"/> </summary>
        public static readonly CharArraySet DEFAULT_ARTICLES = CharArraySet.UnmodifiableSet(new CharArraySet(
#pragma warning disable 612, 618
            LuceneVersion.LUCENE_CURRENT,
#pragma warning restore 612, 618
            new string[] { "l", "m", "t", "qu", "n", "s", "j", "d", "c", "jusqu", "quoiqu", "lorsqu", "puisqu" }, true));

        /// <summary>
        /// Contains words that should be indexed but not stemmed.
        /// </summary>
        private readonly CharArraySet excltable;

        /// <summary>
        /// Returns an unmodifiable instance of the default stop-words set. </summary>
        /// <returns> an unmodifiable instance of the default stop-words set. </returns>
        public static CharArraySet DefaultStopSet
        {
            get
            {
                return DefaultSetHolder.DEFAULT_STOP_SET;
            }
        }

        private class DefaultSetHolder
        {
            /// @deprecated (3.1) remove this in Lucene 5.0, index bw compat 
            [Obsolete("(3.1) remove this in Lucene 5.0, index bw compat")]
            internal static readonly CharArraySet DEFAULT_STOP_SET_30 = CharArraySet.UnmodifiableSet(new CharArraySet(LuceneVersion.LUCENE_CURRENT, Arrays.AsList(FRENCH_STOP_WORDS), false));
            internal static readonly CharArraySet DEFAULT_STOP_SET;
            static DefaultSetHolder()
            {
                try
                {
                    DEFAULT_STOP_SET = WordlistLoader.GetSnowballWordSet(
                        IOUtils.GetDecodingReader(typeof(SnowballFilter), DEFAULT_STOPWORD_FILE, Encoding.UTF8),
#pragma warning disable 612, 618
                        LuceneVersion.LUCENE_CURRENT);
#pragma warning restore 612, 618
                }
                catch (IOException)
                {
                    // default set should always be present as it is part of the
                    // distribution (JAR)
                    throw new Exception("Unable to load default stopword set");
                }
            }
        }

        /// <summary>
        /// Builds an analyzer with the default stop words (<see cref="DefaultStopSet"/>).
        /// </summary>
        public FrenchAnalyzer(LuceneVersion matchVersion)
#pragma warning disable 612, 618
              : this(matchVersion, matchVersion.OnOrAfter(LuceneVersion.LUCENE_31) ?
                    DefaultSetHolder.DEFAULT_STOP_SET : DefaultSetHolder.DEFAULT_STOP_SET_30)
#pragma warning restore 612, 618
        {
        }

        /// <summary>
        /// Builds an analyzer with the given stop words
        /// </summary>
        /// <param name="matchVersion">
        ///          lucene compatibility version </param>
        /// <param name="stopwords">
        ///          a stopword set </param>
        public FrenchAnalyzer(LuceneVersion matchVersion, CharArraySet stopwords)
              : this(matchVersion, stopwords, CharArraySet.EMPTY_SET)
        {
        }

        /// <summary>
        /// Builds an analyzer with the given stop words
        /// </summary>
        /// <param name="matchVersion">
        ///          lucene compatibility version </param>
        /// <param name="stopwords">
        ///          a stopword set </param>
        /// <param name="stemExclutionSet">
        ///          a stemming exclusion set </param>
        public FrenchAnalyzer(LuceneVersion matchVersion, CharArraySet stopwords, CharArraySet stemExclutionSet)
              : base(matchVersion, stopwords)
        {
            this.excltable = CharArraySet.UnmodifiableSet(CharArraySet.Copy(matchVersion, stemExclutionSet));
        }

        /// <summary>
        /// Creates
        /// <see cref="TokenStreamComponents"/>
        /// used to tokenize all the text in the provided <see cref="TextReader"/>.
        /// </summary>
        /// <returns> <see cref="TokenStreamComponents"/>
        ///         built from a <see cref="StandardTokenizer"/> filtered with
        ///         <see cref="StandardFilter"/>, <see cref="ElisionFilter"/>,
        ///         <see cref="LowerCaseFilter"/>, <see cref="StopFilter"/>,
        ///         <see cref="SetKeywordMarkerFilter"/> if a stem exclusion set is
        ///         provided, and <see cref="FrenchLightStemFilter"/> </returns>
        ///         
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
#pragma warning disable 612, 618
            if (m_matchVersion.OnOrAfter(LuceneVersion.LUCENE_31))
#pragma warning restore 612, 618
            {
                Tokenizer source = new StandardTokenizer(m_matchVersion, reader);
                TokenStream result = new StandardFilter(m_matchVersion, source);
                result = new ElisionFilter(result, DEFAULT_ARTICLES);
                result = new LowerCaseFilter(m_matchVersion, result);
                result = new StopFilter(m_matchVersion, result, m_stopwords);
                if (excltable.Count > 0)
                {
                    result = new SetKeywordMarkerFilter(result, excltable);
                }
#pragma warning disable 612, 618
                if (m_matchVersion.OnOrAfter(LuceneVersion.LUCENE_36))
#pragma warning restore 612, 618
                {
                    result = new FrenchLightStemFilter(result);
                }
                else
                {
                    result = new SnowballFilter(result, new FrenchStemmer());
                }
                result = new ASCIIFoldingFilter(result);
                return new TokenStreamComponents(source, new LowerCaseFilter(m_matchVersion, result));
            }
            else
            {
                Tokenizer source = new StandardTokenizer(m_matchVersion, reader);
                TokenStream result = new StandardFilter(m_matchVersion, source);
                result = new StopFilter(m_matchVersion, result, m_stopwords);
                if (excltable.Count > 0)
                {
                    result = new SetKeywordMarkerFilter(result, excltable);
                }
#pragma warning disable 612, 618
                result = new FrenchStemFilter(result);
#pragma warning restore 612, 618
                // Convert to lowercase after stemming!
                return new TokenStreamComponents(source, new LowerCaseFilter(m_matchVersion, result));
            }
        }
    }
}