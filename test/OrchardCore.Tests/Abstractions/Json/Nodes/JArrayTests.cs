using System.Text.Json.Nodes;
using System.Text.Json.Settings;

namespace OrchardCore.Json.Nodes.Test;

public class JArrayTests
{
    public static IEnumerable<object[]> MergeArrayEntries => [
        ["[1, 2, 3, 4]", "[4, 5, 6]", null, "[1,2,3,4,4,5,6]"],
        ["[1, 2, 3, 4]", "[4, 5, 6]", new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Concat }, "[1,2,3,4,4,5,6]"],
        ["[1, 2, 3, 4]", "[4, 5, 6]", new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union }, "[1,2,3,4,5,6]"],
        ["[1, 2, 3, 4]", "[4, 5, 6]", new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Replace }, "[4,5,6]"]
    ];

    [Theory]
    [MemberData(nameof(MergeArrayEntries))]
    public void MergeArrayShouldRespectJsonMergeSettings(string jsonArrayContent1, string jsonArrayContent2, JsonMergeSettings mergeSettings, string expectedJsonString)
    {
        // Arrange
        var array = JsonNode.Parse(jsonArrayContent1) as JsonArray;
        var content = JsonNode.Parse(jsonArrayContent2);

        // Act
        var result = array.Merge(content, mergeSettings);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedJsonString, result.ToJsonString());
    }
}
