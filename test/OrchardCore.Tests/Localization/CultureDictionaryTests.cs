using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class CultureDictionaryTests
{
    [Fact]
    public void MergeAddsRecordToEmptyDictionary_Default_Succeeds()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");

        dictionary.MergeTranslations(new[] { record });

        Assert.Equal(dictionary.Translations[record.Key], record.Translations);
    }

    [Fact]
    public void MergeOverwritesTranslationsForSameKeys_Default_Succeeds()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var record = new CultureDictionaryRecord("ball", "míč", "míče", "míčů");
        var record2 = new CultureDictionaryRecord("ball", "balón", "balóny", "balónů");

        dictionary.MergeTranslations(new[] { record });
        dictionary.MergeTranslations(new[] { record2 });

        Assert.Equal(dictionary.Translations[record.Key], record2.Translations);
    }

    [Fact]
    public void IndexerReturnNullIfKeyDoesntExist_Default_Succeeds()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var key = new CultureDictionaryRecordKey { MessageId = "ball" };
        var translation = dictionary[key];

        Assert.Null(translation);
    }

    [Fact]
    public void IndexerThrowsPluralFormNotFoundExceptionIfSpecifiedPluralFormDoesntExist_Default_Succeeds()
    {
        // Arrange
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var record = new CultureDictionaryRecord("ball", "míč", "míče");
        dictionary.MergeTranslations(new[] { record });

        Assert.Throws<PluralFormNotFoundException>(() =>
        {
            var key = new CultureDictionaryRecordKey { MessageId = "ball" };

            return dictionary[key, 5];
        });
    }

    [Fact]
    public void EnumerateCultureDictionary_Default_Succeeds()
    {
        // Arrange
        var dictionary = new CultureDictionary("ar", PluralizationRule.Arabic);
        dictionary.MergeTranslations(
        [
            new("Hello", "مرحبا"),
            new("Bye", "مع السلامة")
        ]);

        // Act & Assert
        Assert.NotEmpty(dictionary);

        foreach (var record in dictionary)
        {
            Assert.Single(record.Translations);
        }

        Assert.Equal(2, dictionary.Count());
    }

    [Theory]
    [InlineData(1, "jeden")]
    [InlineData(2, "dva")]
    [InlineData(5, "dva")]
    public void Indexer_RecordWithPluralRule_UsesRecordPluralRule(int count, string expected)
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        dictionary.MergeTranslations([new CultureDictionaryRecord("ball", null, ["jeden", "dva"], PluralizationRule.English)]);

        var translation = dictionary[new CultureDictionaryRecordKey { MessageId = "ball" }, count];

        Assert.Equal(expected, translation);
    }

    [Fact]
    public void Indexer_RecordWithoutPluralRule_UsesCulturePluralRule()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        dictionary.MergeTranslations(
        [
            new CultureDictionaryRecord("ball", null, ["jeden", "dva"], PluralizationRule.English),
            new CultureDictionaryRecord("car", "auto", "auta", "aut"),
        ]);

        var key = new CultureDictionaryRecordKey { MessageId = "car" };

        Assert.Equal("aut", dictionary[key, 5]);
        Assert.Same(PluralizationRule.Czech, dictionary.GetPluralRule(key));
        Assert.Same(PluralizationRule.English, dictionary.GetPluralRule(new CultureDictionaryRecordKey { MessageId = "ball" }));
    }

    [Fact]
    public void MergeTranslations_RecordWithoutPluralRuleOverwritesKey_ClearsRecordPluralRule()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var key = new CultureDictionaryRecordKey { MessageId = "ball" };

        dictionary.MergeTranslations([new CultureDictionaryRecord("ball", null, ["jeden", "dva"], PluralizationRule.English)]);
        dictionary.MergeTranslations([new CultureDictionaryRecord("ball", "míč", "míče", "míčů")]);

        Assert.Same(PluralizationRule.Czech, dictionary.GetPluralRule(key));
        Assert.Equal("míčů", dictionary[key, 5]);
    }

    [Fact]
    public void MergeTranslations_RecordWithPluralRuleOverwritesKey_ReplacesRecordPluralRule()
    {
        PluralizationRuleDelegate singleForm = n => 0;
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        var key = new CultureDictionaryRecordKey { MessageId = "ball" };

        dictionary.MergeTranslations([new CultureDictionaryRecord("ball", null, ["jeden", "dva"], PluralizationRule.English)]);
        dictionary.MergeTranslations([new CultureDictionaryRecord("ball", null, ["míč"], singleForm)]);

        Assert.Same(singleForm, dictionary.GetPluralRule(key));
        Assert.Equal("míč", dictionary[key, 5]);
    }

    [Fact]
    public void GetEnumerator_RecordWithPluralRule_ReturnsRecordPluralRule()
    {
        var dictionary = new CultureDictionary("cs", PluralizationRule.Czech);
        dictionary.MergeTranslations(
        [
            new CultureDictionaryRecord("ball", null, ["jeden", "dva"], PluralizationRule.English),
            new CultureDictionaryRecord("car", "auto"),
        ]);

        var records = dictionary.ToDictionary(record => record.Key.MessageId);

        Assert.Same(PluralizationRule.English, records["ball"].PluralRule);
        Assert.Null(records["car"].PluralRule);
    }
}
