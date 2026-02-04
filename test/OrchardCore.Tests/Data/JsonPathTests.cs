using System.Text.Json.Nodes;

namespace OrchardCore.Tests.Data;

/// <summary>
/// Tests for the in-house JSONPath implementation used by <see cref="JNode.SelectNode"/>.
/// </summary>
public class JsonPathTests
{
    #region Happy Path - Property Access

    [Fact]
    public void SelectNode_WithSimplePropertyName_ReturnsPropertyValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John", "age": 30}""");

        // Act
        var result = json.SelectNode("name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithDollarDotPrefix_ReturnsPropertyValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John", "age": 30}""");

        // Act
        var result = json.SelectNode("$.name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithNestedProperty_ReturnsNestedValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John", "address": {"city": "NYC"}}}""");

        // Act
        var result = json.SelectNode("user.name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithDeeplyNestedProperty_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"level1": {"level2": {"level3": {"level4": {"value": "deep"}}}}}""");

        // Act
        var result = json.SelectNode("level1.level2.level3.level4.value");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("deep", result.ToString());
    }

    [Fact]
    public void SelectNode_WithDollarDotNestedPath_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"address": {"city": "NYC"}}}""");

        // Act
        var result = json.SelectNode("$.user.address.city");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("NYC", result.ToString());
    }

    #endregion

    #region Happy Path - Array Access

    [Fact]
    public void SelectNode_WithArrayIndex_ReturnsArrayElement()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[0]");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("a", result.ToString());
    }

    [Fact]
    public void SelectNode_WithArrayIndexInMiddle_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"data": [{"name": "first"}, {"name": "second"}]}""");

        // Act
        var result = json.SelectNode("data[1].name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("second", result.ToString());
    }

    [Fact]
    public void SelectNode_WithMultipleArrayIndices_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"matrix": [[1, 2], [3, 4], [5, 6]]}""");

        // Act
        var result = json.SelectNode("matrix[1][0]");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("3", result.ToString());
    }

    [Fact]
    public void SelectNode_WithArrayAtRoot_ReturnsElement()
    {
        // Arrange
        var json = JsonNode.Parse("""[{"id": 1}, {"id": 2}, {"id": 3}]""");

        // Act
        var result = json.SelectNode("[1].id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2", result.ToString());
    }

    [Fact]
    public void SelectNode_WithLastValidArrayIndex_ReturnsElement()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[2]");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("c", result.ToString());
    }

    #endregion

    #region Happy Path - Recursive Descent

    [Fact]
    public void SelectNode_WithRecursiveDescent_FindsNestedProperty()
    {
        // Arrange
        var json = JsonNode.Parse("""{"outer": {"inner": {"target": "found"}}}""");

        // Act
        var result = json.SelectNode("$..target");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("found", result.ToString());
    }

    [Fact]
    public void SelectNode_WithRecursiveDescent_FindsFirstMatch()
    {
        // Arrange
        var json = JsonNode.Parse("""
            {
                "level1": {
                    "name": "first",
                    "level2": {
                        "name": "second"
                    }
                }
            }
            """);

        // Act
        var result = json.SelectNode("$..name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("first", result.ToString());
    }

    [Fact]
    public void SelectNode_WithRecursiveDescentInArray_FindsProperty()
    {
        // Arrange
        var json = JsonNode.Parse("""
            {
                "items": [
                    {"id": 1, "data": {"value": "target"}},
                    {"id": 2, "data": {"other": "not this"}}
                ]
            }
            """);

        // Act
        var result = json.SelectNode("$..value");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("target", result.ToString());
    }

    [Fact]
    public void SelectNode_WithRecursiveDescentNestedArrays_FindsProperty()
    {
        // Arrange
        var json = JsonNode.Parse("""
            {
                "data": [
                    [{"hidden": "value1"}],
                    [{"hidden": "value2"}]
                ]
            }
            """);

        // Act
        var result = json.SelectNode("$..hidden");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value1", result.ToString());
    }

    #endregion

    #region Happy Path - Root and Empty Paths

    [Fact]
    public void SelectNode_WithDollarOnly_ReturnsRoot()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("$");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(json.ToJsonString(), result.ToJsonString());
    }

    [Fact]
    public void SelectNode_WithDollarDot_ReturnsRoot()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("$.");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(json.ToJsonString(), result.ToJsonString());
    }

    #endregion

    #region Happy Path - Different Value Types

    [Fact]
    public void SelectNode_WithIntegerValue_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"count": 42}""");

        // Act
        var result = json.SelectNode("count");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.GetValue<int>());
    }

    [Fact]
    public void SelectNode_WithBooleanValue_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"active": true}""");

        // Act
        var result = json.SelectNode("active");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.GetValue<bool>());
    }

    [Fact]
    public void SelectNode_WithNullValue_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"value": null}""");

        // Act
        var result = json.SelectNode("value");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithDecimalValue_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"price": 19.99}""");

        // Act
        var result = json.SelectNode("price");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(19.99, result.GetValue<double>(), 0.001);
    }

    [Fact]
    public void SelectNode_WithObjectValue_ReturnsObject()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John", "age": 30}}""");

        // Act
        var result = json.SelectNode("user");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonObject>(result);
        Assert.Equal("John", result["name"]?.ToString());
    }

    [Fact]
    public void SelectNode_WithArrayValue_ReturnsArray()
    {
        // Arrange
        var json = JsonNode.Parse("""{"tags": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("tags");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonArray>(result);
        Assert.Equal(3, result.AsArray().Count);
    }

    #endregion

    #region Edge Cases - Non-Existent Paths

    [Fact]
    public void SelectNode_WithNonExistentProperty_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("age");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithNonExistentNestedProperty_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");

        // Act
        var result = json.SelectNode("user.address.city");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithNonExistentIntermediateProperty_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");

        // Act
        var result = json.SelectNode("nonexistent.name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_RecursiveDescentWithNonExistentProperty_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"a": {"b": {"c": "value"}}}""");

        // Act
        var result = json.SelectNode("$..nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Edge Cases - Array Bounds

    [Fact]
    public void SelectNode_WithArrayIndexOutOfBounds_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[10]");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithNegativeArrayIndex_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[-1]");

        // Assert
        // Note: RFC 9535 defines negative indices as counting from end,
        // but this implementation returns null for negative indices.
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithEmptyArray_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": []}""");

        // Act
        var result = json.SelectNode("items[0]");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithArrayIndexOnNonArray_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("name[0]");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithArrayIndexOnObject_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");

        // Act
        var result = json.SelectNode("user[0]");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Edge Cases - Malformed Paths

    [Fact]
    public void SelectNode_WithUnclosedBracket_StopsAtMalformedPart()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[0");

        // Assert
        // The parser stops at malformed bracket, treating "items" as the path
        Assert.NotNull(result);
        Assert.IsType<JsonArray>(result);
    }

    [Fact]
    public void SelectNode_WithNonNumericArrayIndex_SkipsInvalidSegment()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[abc]");

        // Assert
        // Non-numeric index is skipped, returns the array
        Assert.NotNull(result);
        Assert.IsType<JsonArray>(result);
    }

    [Fact]
    public void SelectNode_WithEmptyBrackets_SkipsEmptySegment()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a", "b", "c"]}""");

        // Act
        var result = json.SelectNode("items[]");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<JsonArray>(result);
    }

    [Fact]
    public void SelectNode_WithConsecutiveDots_HandlesGracefully()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");

        // Act
        var result = json.SelectNode("user..name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithLeadingDot_HandlesGracefully()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode(".name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithTrailingDot_HandlesGracefully()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");

        // Act
        var result = json.SelectNode("user.name.");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    #endregion

    #region Edge Cases - Special Characters in Property Names

    [Fact]
    public void SelectNode_WithUnderscoreInPropertyName_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user_name": "John"}""");

        // Act
        var result = json.SelectNode("user_name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithNumbersInPropertyName_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"item123": "value"}""");

        // Act
        var result = json.SelectNode("item123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result.ToString());
    }

    [Fact]
    public void SelectNode_WithCamelCasePropertyName_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"firstName": "John", "lastName": "Doe"}""");

        // Act
        var result = json.SelectNode("firstName");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    [Fact]
    public void SelectNode_WithPascalCasePropertyName_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"FirstName": "John"}""");

        // Act
        var result = json.SelectNode("FirstName");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.ToString());
    }

    #endregion

    #region Edge Cases - Null and Empty Input

    [Fact]
    public void SelectNode_WithNullNode_ThrowsArgumentNullException()
    {
        // Arrange
        JsonNode json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => json.SelectNode("name"));
    }

    [Fact]
    public void SelectNode_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => json.SelectNode(null!));
    }

    [Fact]
    public void SelectNode_WithEmptyPath_ReturnsRoot()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(json.ToJsonString(), result.ToJsonString());
    }

    [Fact]
    public void SelectNode_WithWhitespacePath_ReturnsRoot()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");

        // Act
        var result = json.SelectNode("   ");

        // Assert
        // The path is trimmed in SelectNode, so whitespace-only paths return the root
        Assert.NotNull(result);
        Assert.Equal(json.ToJsonString(), result.ToJsonString());
    }

    #endregion

    #region Edge Cases - Complex Structures

    [Fact]
    public void SelectNode_WithMixedArrayAndObjectNesting_ReturnsValue()
    {
        // Arrange
        var json = JsonNode.Parse("""
            {
                "users": [
                    {"name": "Alice", "addresses": [{"city": "NYC"}, {"city": "LA"}]},
                    {"name": "Bob", "addresses": [{"city": "Chicago"}]}
                ]
            }
            """);

        // Act
        var result = json.SelectNode("users[0].addresses[1].city");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("LA", result.ToString());
    }

    [Fact]
    public void SelectNode_WithLargeArrayIndex_ReturnsNull()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["a"]}""");

        // Act
        var result = json.SelectNode("items[999999]");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SelectNode_WithZeroIndex_ReturnsFirstElement()
    {
        // Arrange
        var json = JsonNode.Parse("""{"items": ["first", "second"]}""");

        // Act
        var result = json.SelectNode("items[0]");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("first", result.ToString());
    }

    #endregion

    #region Integration Tests - Real-World Scenarios

    [Fact]
    public void SelectNode_ContentItemScenario_ReturnsPartProperty()
    {
        // Arrange - Simulating OrchardCore ContentItem structure
        var json = JsonNode.Parse("""
            {
                "ContentItemId": "abc123",
                "ContentType": "Article",
                "MyPart": {
                    "Text": "Hello World",
                    "myField": {
                        "Value": 42
                    }
                }
            }
            """);

        // Act
        var partResult = json.SelectNode("MyPart");
        var textResult = json.SelectNode("MyPart.Text");
        var fieldResult = json.SelectNode("MyPart.myField.Value");

        // Assert
        Assert.NotNull(partResult);
        Assert.NotNull(textResult);
        Assert.Equal("Hello World", textResult.ToString());
        Assert.NotNull(fieldResult);
        Assert.Equal(42, fieldResult.GetValue<int>());
    }

    [Fact]
    public void SelectNode_RecipeStepScenario_ReturnsDataArrayElement()
    {
        // Arrange - Simulating OrchardCore Recipe structure
        var json = JsonNode.Parse("""
            {
                "name": "content",
                "data": [
                    {
                        "ContentItemId": "item1",
                        "TitlePart": {
                            "Title": "First Article"
                        }
                    },
                    {
                        "ContentItemId": "item2",
                        "TitlePart": {
                            "Title": "Second Article"
                        }
                    }
                ]
            }
            """);

        // Act
        var result = json.SelectNode("data[0].TitlePart.Title");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("First Article", result.ToString());
    }

    [Fact]
    public void SelectNode_UserProfileScenario_ReturnsNestedDisplayName()
    {
        // Arrange - Simulating OrchardCore User Properties structure
        var json = JsonNode.Parse("""
            {
                "UserProfile": {
                    "UserProfile": {
                        "DisplayName": {
                            "Text": "John Doe"
                        }
                    }
                }
            }
            """);

        // Act
        var result = json.SelectNode("$.UserProfile.UserProfile.DisplayName.Text");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.ToString());
    }

    [Fact]
    public void SelectNode_ElasticsearchResponseScenario_ReturnsHitsData()
    {
        // Arrange - Simulating Elasticsearch response structure
        var json = JsonNode.Parse("""
            {
                "hits": {
                    "total": {"value": 100},
                    "hits": [
                        {"_id": "1", "_source": {"title": "Doc 1"}},
                        {"_id": "2", "_source": {"title": "Doc 2"}}
                    ]
                }
            }
            """);

        // Act
        var totalResult = json.SelectNode("hits.total.value");
        var firstHitResult = json.SelectNode("hits.hits[0]._source.title");

        // Assert
        Assert.NotNull(totalResult);
        Assert.Equal(100, totalResult.GetValue<int>());
        Assert.NotNull(firstHitResult);
        Assert.Equal("Doc 1", firstHitResult.ToString());
    }

    [Fact]
    public void SelectNode_RecursiveDescentOnContentItem_FindsText()
    {
        // Arrange - Testing recursive descent as used in ContentItemTests
        var json = JsonNode.Parse("""
            {
                "ContentItemId": "abc123",
                "MyPart": {
                    "Text": "Found via recursive descent"
                }
            }
            """);

        // Act
        var result = json.SelectNode("$..Text");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Found via recursive descent", result.ToString());
    }

    #endregion

    #region Performance Edge Cases

    [Fact]
    public void SelectNode_WithDeeplyNestedStructure_Succeeds()
    {
        // Arrange - Create a deeply nested structure (10 levels)
        var json = JsonNode.Parse("""
            {
                "l1": {"l2": {"l3": {"l4": {"l5": {"l6": {"l7": {"l8": {"l9": {"l10": {"value": "deep"}}}}}}}}}}
            }
            """);

        // Act
        var result = json.SelectNode("l1.l2.l3.l4.l5.l6.l7.l8.l9.l10.value");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("deep", result.ToString());
    }

    [Fact]
    public void SelectNode_WithLargeArray_AccessesCorrectElement()
    {
        // Arrange - Create array with many elements
        var arrayElements = string.Join(",", Enumerable.Range(0, 100).Select(i => $"\"{i}\""));
        var json = JsonNode.Parse($"{{\"items\": [{arrayElements}]}}");

        // Act
        var result = json.SelectNode("items[50]");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("50", result.ToString());
    }

    [Fact]
    public void SelectNode_RecursiveDescentOnWideObject_FindsProperty()
    {
        // Arrange - Create an object with many properties
        var properties = string.Join(",", Enumerable.Range(0, 50).Select(i => $"\"prop{i}\": {{\"nested\": {i}}}"));
        properties += ", \"target\": {\"found\": \"yes\"}";
        var json = JsonNode.Parse($"{{{properties}}}");

        // Act
        var result = json.SelectNode("$..found");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("yes", result.ToString());
    }

    #endregion

    #region TryGetValue Extension Method Tests

    [Fact]
    public void TryGetValue_WithStringValue_ReturnsTrue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");
        var node = json["name"];

        // Act
        var success = node.TryGetValue<string>(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal("John", value);
    }

    [Fact]
    public void TryGetValue_WithIntValue_ReturnsTrue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"age": 30}""");
        var node = json["age"];

        // Act
        var success = node.TryGetValue<int>(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(30, value);
    }

    [Fact]
    public void TryGetValue_WithWrongType_ReturnsFalse()
    {
        // Arrange
        var json = JsonNode.Parse("""{"name": "John"}""");
        var node = json["name"];

        // Act
        var success = node.TryGetValue<int>(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetValue_WithNullNode_ReturnsFalse()
    {
        // Arrange
        JsonNode node = null;

        // Act
        var success = node.TryGetValue<string>(out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_WithObjectNode_ReturnsFalse()
    {
        // Arrange
        var json = JsonNode.Parse("""{"user": {"name": "John"}}""");
        var node = json["user"];

        // Act
        var success = node.TryGetValue<string>(out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_WithBoolValue_ReturnsTrue()
    {
        // Arrange
        var json = JsonNode.Parse("""{"active": true}""");
        var node = json["active"];

        // Act
        var success = node.TryGetValue<bool>(out var value);

        // Assert
        Assert.True(success);
        Assert.True(value);
    }

    #endregion
}
