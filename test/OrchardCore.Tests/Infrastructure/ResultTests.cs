namespace OrchardCore.Infrastructure.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldBeSingleton_WithSucceededTrue_AndNoErrors()
    {
        // Arrange & Act
        var firstResult = Result.Success();
        var secondResult = Result.Success();

        // Assert
        Assert.Same(firstResult, secondResult);
        Assert.True(firstResult.Succeeded);
        Assert.Empty(firstResult.Errors);
    }

    [Fact]
    public void SuccessOfT_ReturnsResultOfT_WithProvidedValue_AndSucceededTrue()
    {
        // Arrange
        var value = "Done!!";

        // Act
        var result = Result.Success(value);

        // Assert
        Assert.IsType<Result<string>>(result);
        Assert.True(result.Succeeded);
        Assert.Equal(value, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failed_NullArray_ReturnsFailed_WithNoErrors()
    {
        // Arrange & Act
        var result = Result.Failed((ResultError[])null);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failed_NoArguments_ReturnsFailed_WithNoErrors()
    {
        // Arrange & Act
        var result = Result.Failed();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failed_WithMultipleErrors_AddsAllErrors()
    {
        // Arrange
        var error1 = new ResultError { Message = new LocalizedString("FirstName", "First Name is required.") };
        var error2 = new ResultError { Message = new LocalizedString("LastName", "Last Name is required.") };

        // Act
        var result = Result.Failed(error1, error2);

        // Assert
        Assert.False(result.Succeeded);

        var errors = result.Errors.ToList();
        Assert.Equal(2, errors.Count);
        Assert.Contains(error1, errors);
        Assert.Contains(error2, errors);
    }

    [Fact]
    public void Failed_WithLocalizedString_CreatesSingleResultError_WithMatchingMessage()
    {
        // Arrange
        var localized = new LocalizedString("key", "message text");

        // Act
        var result = Result.Failed(localized);

        // Assert
        Assert.False(result.Succeeded);

        var errors = result.Errors.ToList();
        Assert.Single(errors);
        Assert.Equal("message text", errors[0].Message.Value);
    }

    [Fact]
    public void FailedT_ReturnsResultT_WithDefaultValue_AndProvidedErrors()
    {
        // Arrange
        var error = new ResultError { Message = new LocalizedString("key", "message text") };

        // Act
        var result = Result.Failed<string>(error);

        // Assert
        Assert.IsType<Result<string>>(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Value);

        var errors = result.Errors.ToList();
        Assert.Single(errors);
        Assert.Equal("message text", errors[0].Message.Value);
    }
}
