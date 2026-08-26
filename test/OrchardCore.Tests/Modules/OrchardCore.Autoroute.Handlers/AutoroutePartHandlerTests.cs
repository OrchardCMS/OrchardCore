using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Routing;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Autoroute.Handlers;

public class AutoroutePartHandlerTests
{
    [Fact]
    public async Task GenerateRelativeUniquePathAsync_ShouldReturnUniquePath_WhenPathAlreadyExists()
    {
        // Arrange
        var expectedPath = "existing-path";
        var contextContentItemId = "context-item-id";
        var existingEntries = new List<AutorouteEntry>
        {
            new AutorouteEntry("id", expectedPath, "other-item-id"),
            new AutorouteEntry("id", $"{expectedPath}-1", "other-item-id"),
        };

        var context = new AutoroutePart
        {
            ContentItem = new global::OrchardCore.ContentManagement.ContentItem
            {
                ContentItemId = contextContentItemId,
            },
        };

        // Act
        var actualPath = AutoroutePartHandler.GenerateRelativeUniquePath(existingEntries, expectedPath, context);

        // Assert
        Assert.Equal($"{expectedPath}-2", actualPath);
    }
    [Fact]
    public async Task GenerateRelativeUniquePathAsync_ConcurrentCalls_ReturnsUniquePaths()
    {
        // Arrange
        var basePath = "test-path";
        var contextContentItemId = "context-item-id";
        var existingEntries = new List<AutorouteEntry>
        {
            new AutorouteEntry("id", basePath, "other-item-id"),
            new AutorouteEntry("id", $"{basePath}-1", "other-item-id"),
        };

        var context = new AutoroutePart
        {
            ContentItem = new global::OrchardCore.ContentManagement.ContentItem
            {
                ContentItemId = contextContentItemId,
            },
        };

        // Act: call the method multiple times concurrently sharing the SAME entries list
        var sharedEntries = new List<AutorouteEntry>(existingEntries);
        var tasks = Enumerable.Range(0, 5).Select(i => Task.Run(() =>
            AutoroutePartHandler.GenerateRelativeUniquePath(sharedEntries, basePath, context))
        ).ToArray();

        await Task.WhenAll(tasks);

        var results = tasks.Select(t => t.Result).ToArray();

        // Assert
        Assert.Equal(5, results.Length);
        Assert.All(results, r => Assert.StartsWith($"{basePath}-", r));
        Assert.Distinct(results);
    }
    [Fact]
    public void Test1() {
        Assert.True(true);
    }
}

public class AutoroutePartExtensionsTests
{
    private readonly Mock<IStringLocalizer> _localizerMock = new();

    [Fact]
    public void ValidatePathFieldValue_ReturnsError_WhenPathIsHomepage()
    {
        // Arrange
        var part = new AutoroutePart { Path = "/" };
        _localizerMock.Setup(l => l["Your permalink can't be set to the homepage, please use the homepage option instead."])
            .Returns(new LocalizedString("Your permalink can't be set to the homepage, please use the homepage option instead.", "Your permalink can't be set to the homepage, please use the homepage option instead."));

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(nameof(part.Path), results[0].MemberNames.Single());
    }

    [Theory]
    [InlineData("test:path")]
    [InlineData("test?path")]
    [InlineData("test#path")]
    [InlineData("test[path")]
    [InlineData("test]path")]
    [InlineData("test@path")]
    [InlineData("test!path")]
    [InlineData("test$path")]
    [InlineData("test&path")]
    [InlineData("test'path")]
    [InlineData("test(path")]
    [InlineData("test)path")]
    [InlineData("test*path")]
    [InlineData("test+path")]
    [InlineData("test,path")]
    [InlineData("test.path")]
    [InlineData("test;path")]
    [InlineData("test=path")]
    [InlineData("test<path")]
    [InlineData("test>path")]
    [InlineData("test\\path")]
    [InlineData("test|path")]
    [InlineData("test%path")]
    public void ValidatePathFieldValue_ReturnsError_WhenPathContainsInvalidCharacters(string invalidPath)
    {
        // Arrange
        var part = new AutoroutePart { Path = invalidPath };
        var invalidChars = string.Join(", ", AutoroutePart.InvalidCharactersForPath.Select(c => $"\"{c}\""));
        _localizerMock.Setup(l => l[It.Is<string>(s => s.Contains("Please do not use any of the following characters"))])
            .Returns(new LocalizedString($"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", $"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead)."));

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(nameof(part.Path), results[0].MemberNames.Single());
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsError_WhenPathContainsSpace()
    {
        // Arrange
        var part = new AutoroutePart { Path = "test path" };
        var invalidChars = string.Join(", ", AutoroutePart.InvalidCharactersForPath.Select(c => $"\"{c}\""));
        _localizerMock.Setup(l => l[It.Is<string>(s => s.Contains("Please do not use any of the following characters"))])
            .Returns(new LocalizedString($"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", $"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead)."));

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsError_WhenPathContainsConsecutiveSlashes()
    {
        // Arrange
        var part = new AutoroutePart { Path = "test//path" };
        var invalidChars = string.Join(", ", AutoroutePart.InvalidCharactersForPath.Select(c => $"\"{c}\""));
        _localizerMock.Setup(l => l[It.Is<string>(s => s.Contains("Please do not use any of the following characters"))])
            .Returns(new LocalizedString($"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", $"Please do not use any of the following characters in your permalink: {invalidChars}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead)."));

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsError_WhenPathExceedsMaxLength()
    {
        // Arrange
        var part = new AutoroutePart { Path = new string('a', AutoroutePart.MaxPathLength + 1) };
        _localizerMock.Setup(l => l[It.Is<string>(s => s.Contains("Your permalink is too long"))])
            .Returns(new LocalizedString($"Your permalink is too long. The permalink can only be up to {AutoroutePart.MaxPathLength} characters.", $"Your permalink is too long. The permalink can only be up to {AutoroutePart.MaxPathLength} characters."));

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(nameof(part.Path), results[0].MemberNames.Single());
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsNoErrors_WhenPathIsValid()
    {
        // Arrange
        var part = new AutoroutePart { Path = "valid-path" };

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsNoErrors_WhenPathIsEmpty()
    {
        // Arrange
        var part = new AutoroutePart { Path = "" };

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ValidatePathFieldValue_ReturnsNoErrors_WhenPathIsNull()
    {
        // Arrange
        var part = new AutoroutePart { Path = null };

        // Act
        var results = part.ValidatePathFieldValue(_localizerMock.Object).ToList();

        // Assert
        Assert.Empty(results);
    }
}