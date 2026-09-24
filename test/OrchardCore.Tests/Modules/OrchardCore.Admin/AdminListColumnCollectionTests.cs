using OrchardCore.Admin.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListColumnCollectionTests
{
    [Fact]
    public void Add_NameTheListAlreadyHas_ThrowsWhateverTheCase()
    {
        var columns = new AdminListColumnCollection { new AdminListColumn { Name = "Title" } };

        var exception = Assert.Throws<ArgumentException>(() => columns.Add(new AdminListColumn { Name = "title" }));

        Assert.Contains("already has a 'title' column", exception.Message);
        Assert.Single(columns);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Add_ColumnWithoutName_Throws(string name)
    {
        var columns = new AdminListColumnCollection();

        Assert.Throws<ArgumentException>(() => columns.Add(new AdminListColumn { Name = name }));
        Assert.Empty(columns);
    }

    [Fact]
    public void Add_SeveralColumns_KeepsTheOrderTheyWereAddedIn()
    {
        var columns = new AdminListColumnCollection
        {
            new AdminListColumn { Name = "Title" },
            new AdminListColumn { Name = "Actions" },
            new AdminListColumn { Name = "Culture" },
        };

        Assert.Equal(["Title", "Actions", "Culture"], columns.Select(column => column.Name));
    }

    [Fact]
    public void TryGetValueAndRemove_ByName_IgnoreTheCase()
    {
        var title = new AdminListColumn { Name = "Title" };
        var columns = new AdminListColumnCollection { title, new AdminListColumn { Name = "Actions" } };

        Assert.True(columns.TryGetValue("TITLE", out var found));
        Assert.Same(title, found);

        Assert.True(columns.Remove("title"));
        Assert.False(columns.Contains("Title"));
        Assert.Equal(["Actions"], columns.Select(column => column.Name));

        // The name is free again once its column is removed.
        columns.Add(new AdminListColumn { Name = "Title" });
        Assert.Equal(["Actions", "Title"], columns.Select(column => column.Name));
    }

    [Fact]
    public void SetItem_ColumnWithoutName_Throws()
    {
        var columns = new AdminListColumnCollection { new AdminListColumn { Name = "Title" } };

        Assert.Throws<ArgumentException>(() => columns[0] = new AdminListColumn());
        Assert.Equal("Title", columns[0].Name);
    }
}
