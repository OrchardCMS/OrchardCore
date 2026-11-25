using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using OrchardCore.Localization;
using OrchardCore.Localization.DataAnnotations;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class LocalizedValidationMetadataProviderTests
{
    private const string Culture = "fr";
    private const string ColorPattern = "^#[0-9a-fA-F]{6}$";
    private const string LocalizerContext = "OrchardCore.Localization.DataAnnotations.LocalizedDataAnnotationsMvcOptions";
    private const string DefaultErrorMessagesContext = "OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages";

    [Theory]
    [InlineData(nameof(Model.UserName))]
    [InlineData(nameof(Model.Age))]
    [InlineData(nameof(Model.Score))]
    [InlineData(nameof(Model.Code))]
    [InlineData(nameof(Model.Nickname))]
    [InlineData(nameof(Model.MinimumLengthTitle))]
    [InlineData(nameof(Model.MaximumLengthTitle))]
    [InlineData(nameof(Model.Color))]
    [InlineData(nameof(Model.Background))]
    [InlineData(nameof(Model.Email))]
    [InlineData(nameof(Model.ConfirmPassword))]
    [InlineData(nameof(Model.Title))]
    [InlineData(nameof(Model.Subtitle))]
    public void CreateValidationMetadata_NoTranslation_KeepsDefaultErrorMessage(string propertyName)
    {
        using var scope = CultureScope.Create(Culture);

        var (expected, _) = CreateAttribute(propertyName);
        var (attribute, argument) = CreateLocalizedAttribute(propertyName);

        Assert.Equal(expected.FormatErrorMessage(argument), attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void Validate_RegularExpressionWithQuantifierAndNoTranslation_ReturnsDefaultErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, _) = CreateLocalizedAttribute(nameof(Model.Color));

        var result = Validate(attribute, nameof(Model.Color), "#12");

        Assert.Equal($"The field Color must match the regular expression '{ColorPattern}'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_RegularExpressionWithQuantifierAndTranslatedTemplate_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, _) = CreateLocalizedAttribute(
            nameof(Model.Color),
            new CultureDictionaryRecord(
                "The field {0} must match the regular expression '{1}'.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit correspondre à l'expression régulière '{1}'."]));

        var result = Validate(attribute, nameof(Model.Color), "#12");

        Assert.Equal($"Le champ Color doit correspondre à l'expression régulière '{ColorPattern}'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_RegularExpressionWithQuantifierAndTranslatedFormattedMessage_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, _) = CreateLocalizedAttribute(
            nameof(Model.Color),
            new CultureDictionaryRecord(
                $"The field Color must match the regular expression '{ColorPattern}'.",
                ["La couleur doit correspondre à '^#[0-9a-fA-F]{6}$'."]));

        var result = Validate(attribute, nameof(Model.Color), "#12");

        Assert.Equal("La couleur doit correspondre à '^#[0-9a-fA-F]{6}$'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_RegularExpressionWithQuantifierAndTranslatedUnparameterizedMessage_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, _) = CreateLocalizedAttribute(
            nameof(Model.Color),
            new CultureDictionaryRecord(
                $"The field {{0}} must match the regular expression '{ColorPattern}'.",
                ["Le champ {0} doit correspondre à '^#[0-9a-fA-F]{6}$'."]));

        var result = Validate(attribute, nameof(Model.Color), "#12");

        Assert.Equal($"Le champ Color doit correspondre à '{ColorPattern}'.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_RegularExpressionWithDisplayName_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Background),
            new CultureDictionaryRecord(
                "The field {0} must match the regular expression '{1}'.",
                ["Le champ {0} doit correspondre à l'expression régulière '{1}'."]));

        var result = Validate(attribute, argument, "#12");

        Assert.Equal($"Le champ Background color doit correspondre à l'expression régulière '{ColorPattern}'.", result.ErrorMessage);
    }

    [Fact]
    public void FormatErrorMessage_RequiredWithTranslation_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.UserName),
            new CultureDictionaryRecord("The {0} field is required.", DefaultErrorMessagesContext, ["Le champ {0} est requis."]));

        Assert.Equal("Le champ UserName est requis.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_RequiredWithTranslatedFormattedMessage_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.UserName),
            new CultureDictionaryRecord("The UserName field is required.", ["Le nom d'utilisateur est requis."]));

        Assert.Equal("Le nom d'utilisateur est requis.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_RangeWithTranslation_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Age),
            new CultureDictionaryRecord(
                "The field {0} must be between {1} and {2}.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit être compris entre {1} et {2}."]));

        Assert.Equal("Le champ Age doit être compris entre 1 et 10.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_ExclusiveRangeWithInclusiveTranslation_KeepsExclusiveErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (expected, _) = CreateAttribute(nameof(Model.Score));
        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Score),
            new CultureDictionaryRecord(
                "The field {0} must be between {1} and {2}.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit être compris entre {1} et {2}."]));

        Assert.Equal(expected.FormatErrorMessage(argument), attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_StringLengthWithTranslation_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Code),
            new CultureDictionaryRecord(
                "The field {0} must be a string with a maximum length of {1}.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit contenir au plus {1} caractères."]));

        Assert.Equal("Le champ Code doit contenir au plus 10 caractères.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_StringLengthWithMinimumAndTranslation_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Nickname),
            new CultureDictionaryRecord(
                "The field {0} must be a string with a maximum length of {1}.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit contenir au plus {1} caractères."]),
            new CultureDictionaryRecord(
                "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.",
                DefaultErrorMessagesContext,
                ["Le champ {0} doit contenir entre {2} et {1} caractères."]));

        Assert.Equal("Le champ Nickname doit contenir entre 2 et 10 caractères.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void Validate_MinLengthWithTranslatedTemplate_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.MinimumLengthTitle),
            new CultureDictionaryRecord(
                "The field {0} must be a string or array type with a minimum length of '{1}'.",
                DefaultErrorMessagesContext,
                ["{0} must be at least {1} characters."]));

        var result = Validate(attribute, argument, "Test");

        Assert.Equal("Title must be at least 5 characters.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_MaxLengthWithTranslatedTemplate_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.MaximumLengthTitle),
            new CultureDictionaryRecord(
                "The field {0} must be a string or array type with a maximum length of '{1}'.",
                DefaultErrorMessagesContext,
                ["{0} must be {1} characters or fewer."]));

        var result = Validate(attribute, argument, new string('a', 51));

        Assert.Equal("Title must be 50 characters or fewer.", result.ErrorMessage);
    }

    [Fact]
    public void FormatErrorMessage_CustomErrorMessageWithTranslation_ReturnsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(
            nameof(Model.Title),
            new CultureDictionaryRecord("Please enter the {0}.", DefaultErrorMessagesContext, ["Veuillez saisir le {0}."]));

        Assert.Equal("Veuillez saisir le Title.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void FormatErrorMessage_ResourceErrorMessage_KeepsResourceErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var (attribute, argument) = CreateLocalizedAttribute(nameof(Model.Subtitle));

        Assert.Null(attribute.ErrorMessage);
        Assert.Equal("The Subtitle is required by the resource.", attribute.FormatErrorMessage(argument));
    }

    [Fact]
    public void CreateValidationMetadata_CalledTwice_KeepsLocalizedErrorMessage()
    {
        using var scope = CultureScope.Create(Culture);

        var localizer = CreateLocalizer(
            new CultureDictionaryRecord(
                "The field {0} must match the regular expression '{1}'.",
                ["Le champ {0} doit correspondre à l'expression régulière '{1}'."]));

        var (attribute, argument) = CreateLocalizedAttribute(nameof(Model.Color), localizer);
        ApplyProvider(attribute, nameof(Model.Color), localizer);

        Assert.Equal(
            $"Le champ Color doit correspondre à l'expression régulière '{ColorPattern}'.",
            attribute.FormatErrorMessage(argument));
    }

    private static (ValidationAttribute Attribute, string Argument) CreateLocalizedAttribute(string propertyName, params CultureDictionaryRecord[] records)
        => CreateLocalizedAttribute(propertyName, CreateLocalizer(records));

    private static (ValidationAttribute Attribute, string Argument) CreateLocalizedAttribute(string propertyName, PortableObjectStringLocalizer localizer)
    {
        var (attribute, argument) = CreateAttribute(propertyName);

        ApplyProvider(attribute, propertyName, localizer);

        return (attribute, argument);
    }

    // Reflection creates new attribute instances on each call, so that each test changes its own instances.
    private static (ValidationAttribute Attribute, string Argument) CreateAttribute(string propertyName)
    {
        var property = typeof(Model).GetProperty(propertyName);
        var attributes = ModelAttributes.GetAttributesForProperty(typeof(Model), property);
        var argument = attributes.Attributes.OfType<DisplayAttribute>().FirstOrDefault()?.Name ?? propertyName;

        return (attributes.Attributes.OfType<ValidationAttribute>().Single(), argument);
    }

    private static void ApplyProvider(ValidationAttribute attribute, string propertyName, PortableObjectStringLocalizer localizer)
    {
        var property = typeof(Model).GetProperty(propertyName);
        var attributes = ModelAttributes.GetAttributesForProperty(typeof(Model), property);
        var context = new ValidationMetadataProviderContext(
            ModelMetadataIdentity.ForProperty(property, property.PropertyType, typeof(Model)),
            attributes);

        context.ValidationMetadata.ValidatorMetadata.Add(attribute);

        new LocalizedValidationMetadataProvider(localizer).CreateValidationMetadata(context);
    }

    private static PortableObjectStringLocalizer CreateLocalizer(params CultureDictionaryRecord[] records)
    {
        var dictionary = new CultureDictionary(Culture, PluralizationRule.English);
        dictionary.MergeTranslations(records);

        var localizationManager = new Mock<ILocalizationManager>();
        localizationManager
            .Setup(manager => manager.GetDictionary(It.Is<CultureInfo>(culture => culture.Name == Culture)))
            .Returns(dictionary);

        return new PortableObjectStringLocalizer(LocalizerContext, localizationManager.Object, false, Mock.Of<ILogger>());
    }

    private static ValidationResult Validate(ValidationAttribute attribute, string displayName, object value)
        => attribute.GetValidationResult(value, new ValidationContext(new Model()) { DisplayName = displayName });

    public static class Resources
    {
        public static string SubtitleRequired => "The {0} is required by the resource.";
    }

    public sealed class Model
    {
        [Required]
        public string UserName { get; set; }

        [Range(1, 10)]
        public int Age { get; set; }

        [Range(1, 10, MinimumIsExclusive = true)]
        public int Score { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        [StringLength(10, MinimumLength = 2)]
        public string Nickname { get; set; }

        [RegularExpression(ColorPattern)]
        public string Color { get; set; }

        [MinLength(5)]
        [Display(Name = "Title")]
        public string MinimumLengthTitle { get; set; }

        [MaxLength(50)]
        [Display(Name = "Title")]
        public string MaximumLengthTitle { get; set; }

        [Display(Name = "Background color")]
        [RegularExpression(ColorPattern)]
        public string Background { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please enter the {0}.")]
        public string Title { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.SubtitleRequired), ErrorMessageResourceType = typeof(Resources))]
        public string Subtitle { get; set; }
    }
}
