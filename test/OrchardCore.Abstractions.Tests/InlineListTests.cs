using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OrchardCore;

public class InlineListTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldCreateEmptyList()
    {
        // Arrange & Act
        var list = new InlineList<int>();

        // Assert
        Assert.Empty(list);
        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithSpan_WithinInlineCapacity()
    {
        // Arrange
        ReadOnlySpan<int> items = [1, 2, 3, 4, 5];

        // Act
        var list = new InlineList<int>(items);

        // Assert
        Assert.Equal(5, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
        Assert.Equal(5, list[4]);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithSpan_ExactlyInlineCapacity()
    {
        // Arrange
        ReadOnlySpan<int> items = [1, 2, 3, 4, 5, 6, 7, 8];

        // Act
        var list = new InlineList<int>(items);

        // Assert
        Assert.Equal(8, list.Count);
        for (int i = 0; i < 8; i++)
        {
            Assert.Equal(i + 1, list[i]);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeWithSpan_ExceedingInlineCapacity()
    {
        // Arrange
        ReadOnlySpan<int> items = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        // Act
        var list = new InlineList<int>(items);

        // Assert
        Assert.Equal(10, list.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i + 1, list[i]);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptySpan()
    {
        // Arrange
        ReadOnlySpan<int> items = [];

        // Act
        var list = new InlineList<int>(items);

        // Assert
        Assert.Empty(list);
    }

    #endregion

    #region Indexer Tests

    [Fact]
    public void Indexer_ShouldGetAndSetValues_InInlineStorage()
    {
        // Arrange
        var list = new InlineList<string>();
        list.Add("a");
        list.Add("b");
        list.Add("c");

        // Act
        var value = list[1];
        list[1] = "modified";

        // Assert
        Assert.Equal("b", value);
        Assert.Equal("modified", list[1]);
    }

    [Fact]
    public void Indexer_ShouldGetAndSetValues_InOverflowStorage()
    {
        // Arrange
        var list = new InlineList<int>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(i);
        }

        // Act
        var value = list[8];
        list[8] = 999;

        // Assert
        Assert.Equal(8, value);
        Assert.Equal(999, list[8]);
    }

    [Fact]
    public void Indexer_ShouldThrowException_WhenIndexIsNegative()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]);
    }

    [Fact]
    public void Indexer_ShouldThrowException_WhenIndexIsGreaterThanOrEqualCount()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => list[3]);
    }

    [Fact]
    public void Indexer_Set_ShouldThrowException_WhenIndexIsOutOfRange()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list[1] = 10);
    }

    #endregion

    #region Add Tests

    [Fact]
    public void Add_ShouldAddItemsWithinInlineCapacity()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(5, list.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void Add_ShouldAddItemsExactlyAtInlineCapacity()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        for (int i = 0; i < 8; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(8, list.Count);
        for (int i = 0; i < 8; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void Add_ShouldTransitionToOverflowStorage_WhenExceedingInlineCapacity()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(10, list.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void Add_ShouldExpandOverflowStorage_WhenNeeded()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act - Add more items than initial overflow capacity (8 inline + 8 initial overflow = 16)
        for (int i = 0; i < 20; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(20, list.Count);
        for (int i = 0; i < 20; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void Add_ShouldHandleNullValues()
    {
        // Arrange
        var list = new InlineList<string?>();

        // Act
        list.Add("value");
        list.Add(null);
        list.Add("another");

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal("value", list[0]);
        Assert.Null(list[1]);
        Assert.Equal("another", list[2]);
    }

    #endregion

    #region Insert Tests

    [Fact]
    public void Insert_ShouldInsertAtBeginning()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        list.Insert(0, 0);

        // Assert
        Assert.Equal(4, list.Count);
        Assert.Equal(0, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(2, list[2]);
        Assert.Equal(3, list[3]);
    }

    [Fact]
    public void Insert_ShouldInsertAtMiddle()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(4);

        // Act
        list.Insert(2, 3);

        // Assert
        Assert.Equal(4, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
        Assert.Equal(4, list[3]);
    }

    [Fact]
    public void Insert_ShouldInsertAtEnd_CallsAdd()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);

        // Act
        list.Insert(2, 3);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Insert_ShouldTransitionToOverflow_WhenAtCapacity()
    {
        // Arrange
        var list = new InlineList<int>();
        for (int i = 0; i < 8; i++)
        {
            list.Add(i);
        }

        // Act
        list.Insert(4, 99);

        // Assert
        Assert.Equal(9, list.Count);
        Assert.Equal(99, list[4]);
    }

    [Fact]
    public void Insert_ShouldExpandOverflow_WhenNeeded()
    {
        // Arrange
        var list = new InlineList<int>();
        for (int i = 0; i < 16; i++)
        {
            list.Add(i);
        }

        // Act
        list.Insert(8, 99);

        // Assert
        Assert.Equal(17, list.Count);
        Assert.Equal(99, list[8]);
    }

    [Fact]
    public void Insert_ShouldThrowException_WhenIndexIsNegative()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(-1, 10));
    }

    [Fact]
    public void Insert_ShouldThrowException_WhenIndexIsGreaterThanCount()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(2, 10));
    }

    #endregion

    #region RemoveAt Tests

    [Fact]
    public void RemoveAt_ShouldRemoveFirstItem()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        list.RemoveAt(0);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0]);
        Assert.Equal(3, list[1]);
    }

    [Fact]
    public void RemoveAt_ShouldRemoveMiddleItem()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        list.RemoveAt(1);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(3, list[1]);
    }

    [Fact]
    public void RemoveAt_ShouldRemoveLastItem()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        list.RemoveAt(2);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void RemoveAt_ShouldWorkWithOverflowStorage()
    {
        // Arrange
        var list = new InlineList<int>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(i);
        }

        // Act
        list.RemoveAt(5);

        // Assert
        Assert.Equal(9, list.Count);
        Assert.Equal(6, list[5]);
    }

    [Fact]
    public void RemoveAt_ShouldThrowException_WhenIndexIsNegative()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
    }

    [Fact]
    public void RemoveAt_ShouldThrowException_WhenIndexIsGreaterThanOrEqualCount()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(1));
    }

    [Fact]
    public void RemoveAt_ShouldThrowException_OnEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ShouldRemoveFirstOccurrence()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(2);
        list.Add(3);

        // Act
        var result = list.Remove(2);

        // Assert
        Assert.True(result);
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void Remove_ShouldReturnFalse_WhenItemNotFound()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        var result = list.Remove(99);

        // Assert
        Assert.False(result);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void Remove_ShouldReturnFalse_OnEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        var result = list.Remove(1);

        // Assert
        Assert.False(result);
        Assert.Empty(list);
    }

    [Fact]
    public void Remove_ShouldHandleNullValues()
    {
        // Arrange
        var list = new InlineList<string?>();
        list.Add("a");
        list.Add(null);
        list.Add("b");

        // Act
        var result = list.Remove(null);

        // Assert
        Assert.True(result);
        Assert.Equal(2, list.Count);
        Assert.Equal("a", list[0]);
        Assert.Equal("b", list[1]);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_ShouldResetCount()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        list.Clear();

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void Clear_ShouldAllowReuse()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Clear();

        // Act
        list.Add(3);
        list.Add(4);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Equal(3, list[0]);
        Assert.Equal(4, list[1]);
    }

    [Fact]
    public void Clear_ShouldWorkOnEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        list.Clear();

        // Assert
        Assert.Empty(list);
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act & Assert
        Assert.True(list.Contains(2));
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act & Assert
        Assert.False(list.Contains(99));
    }

    [Fact]
    public void Contains_ShouldReturnFalse_OnEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act & Assert
        Assert.False(list.Contains(1));
    }

    [Fact]
    public void Contains_ShouldHandleNullValues()
    {
        // Arrange
        var list = new InlineList<string?>();
        list.Add("a");
        list.Add(null);
        list.Add("b");

        // Act & Assert
        Assert.True(list.Contains(null));
    }

    #endregion

    #region IndexOf Tests

    [Fact]
    public void IndexOf_ShouldReturnIndex_WhenItemExists()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        var index = list.IndexOf(2);

        // Assert
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_ShouldReturnMinusOne_WhenItemDoesNotExist()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        var index = list.IndexOf(99);

        // Assert
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_ShouldReturnFirstIndex_WhenDuplicatesExist()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(2);
        list.Add(3);

        // Act
        var index = list.IndexOf(2);

        // Assert
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_ShouldReturnMinusOne_OnEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        var index = list.IndexOf(1);

        // Assert
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_ShouldHandleNullValues()
    {
        // Arrange
        var list = new InlineList<string?>();
        list.Add("a");
        list.Add(null);
        list.Add("b");

        // Act
        var index = list.IndexOf(null);

        // Assert
        Assert.Equal(1, index);
    }

    #endregion

    #region CopyTo Tests

    [Fact]
    public void CopyTo_Span_ShouldCopyAllItems()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        Span<int> destination = new int[5];

        // Act
        list.CopyTo(destination);

        // Assert
        Assert.Equal(1, destination[0]);
        Assert.Equal(2, destination[1]);
        Assert.Equal(3, destination[2]);
        Assert.Equal(0, destination[3]);
        Assert.Equal(0, destination[4]);
    }

    [Fact]
    public void CopyTo_Span_ShouldThrowException_WhenDestinationTooSmall()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        Span<int> destination = new int[2];

        // Act & Assert
        var exception = false;
        try
        {
            list.CopyTo(destination);
        }
        catch (ArgumentException)
        {
            exception = true;
        }

        Assert.True(exception, "Expected ArgumentException was not thrown");
    }

    [Fact]
    public void CopyTo_Array_ShouldCopyAllItems()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        var destination = new int[5];

        // Act
        list.CopyTo(destination, 0);

        // Assert
        Assert.Equal(1, destination[0]);
        Assert.Equal(2, destination[1]);
        Assert.Equal(3, destination[2]);
        Assert.Equal(0, destination[3]);
        Assert.Equal(0, destination[4]);
    }

    [Fact]
    public void CopyTo_Array_ShouldCopyWithOffset()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        var destination = new int[5];

        // Act
        list.CopyTo(destination, 2);

        // Assert
        Assert.Equal(0, destination[0]);
        Assert.Equal(0, destination[1]);
        Assert.Equal(1, destination[2]);
        Assert.Equal(2, destination[3]);
        Assert.Equal(3, destination[4]);
    }

    [Fact]
    public void CopyTo_Array_ShouldThrowException_WhenArrayIsNull()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));
    }

    [Fact]
    public void CopyTo_Array_ShouldThrowException_WhenArrayIndexIsInvalid()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        var destination = new int[5];

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(destination, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(destination, 10));
    }

    #endregion

    #region Enumerator Tests

    [Fact]
    public void GetEnumerator_ShouldEnumerateAllItems()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        var items = new List<int>();
        foreach (var item in list)
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0]);
        Assert.Equal(2, items[1]);
        Assert.Equal(3, items[2]);
    }

    [Fact]
    public void GetEnumerator_ShouldWorkWithEmptyList()
    {
        // Arrange
        var list = new InlineList<int>();

        // Act
        var items = new List<int>();
        foreach (var item in list)
        {
            items.Add(item);
        }

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetEnumerator_ShouldWorkWithOverflowStorage()
    {
        // Arrange
        var list = new InlineList<int>();
        for (int i = 0; i < 10; i++)
        {
            list.Add(i);
        }

        // Act
        var items = new List<int>();
        foreach (var item in list)
        {
            items.Add(item);
        }

        // Assert
        Assert.Equal(10, items.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i, items[i]);
        }
    }

    [Fact]
    public void Enumerator_Current_ShouldReturnCurrentItem()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        using var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.MoveNext();
        var second = enumerator.Current;

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void Enumerator_Reset_ShouldResetPosition()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);

        // Act
        using var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        enumerator.MoveNext();
        enumerator.Reset();
        enumerator.MoveNext();

        // Assert
        Assert.Equal(1, enumerator.Current);
    }

    [Fact]
    public void GetEnumerator_NonGeneric_ShouldWork()
    {
        // Arrange
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Act
        var items = new List<int>();
        var enumerable = (System.Collections.IEnumerable)list;
        foreach (var item in enumerable)
        {
            items.Add((int)item);
        }

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0]);
        Assert.Equal(2, items[1]);
        Assert.Equal(3, items[2]);
    }

    #endregion

    #region IsReadOnly Tests

    [Fact]
    public void IsReadOnly_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var list = new InlineList<int>();

        // Assert
        Assert.False(list.IsReadOnly);

        // Add items and check again
        list.Add(1);
        Assert.False(list.IsReadOnly);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ComplexScenario_ShouldHandleMixedOperations()
    {
        // Arrange
        var list = new InlineList<string>();

        // Act & Assert - Build up list
        list.Add("a");
        list.Add("b");
        list.Add("c");
        Assert.Equal(3, list.Count);

        // Insert in middle
        list.Insert(1, "x");
        Assert.Equal(4, list.Count);
        Assert.Equal("x", list[1]);

        // Remove first
        list.RemoveAt(0);
        Assert.Equal(3, list.Count);
        Assert.Equal("x", list[0]);

        // Add more to exceed inline capacity
        for (int i = 0; i < 7; i++)
        {
            list.Add($"item{i}");
        }
        Assert.Equal(10, list.Count);

        // Remove by value
        Assert.True(list.Remove("x"));
        Assert.Equal(9, list.Count);

        // Clear
        list.Clear();
        Assert.Empty(list);
    }

    [Fact]
    public void StressTest_ShouldHandleManyItems()
    {
        // Arrange
        var list = new InlineList<int>();
        const int itemCount = 1000;

        // Act - Add many items
        for (int i = 0; i < itemCount; i++)
        {
            list.Add(i);
        }

        // Assert
        Assert.Equal(itemCount, list.Count);
        for (int i = 0; i < itemCount; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void ValueTypeSemantics_ShouldCopyOnAssignment()
    {
        // Arrange
        var list1 = new InlineList<int>();
        list1.Add(1);
        list1.Add(2);

        // Act - Copy the struct
        var list2 = list1;
        list2.Add(3);

        // Assert - list1 should be unchanged
        Assert.Equal(2, list1.Count);
        Assert.Equal(3, list2.Count);
    }

    [Fact]
    public void WorksWithReferenceTypes()
    {
        // Arrange
        var list = new InlineList<object>();
        var obj1 = new object();
        var obj2 = new object();

        // Act
        list.Add(obj1);
        list.Add(obj2);

        // Assert
        Assert.Equal(2, list.Count);
        Assert.Same(obj1, list[0]);
        Assert.Same(obj2, list[1]);
    }

    [Fact]
    public void WorksWithValueTypes()
    {
        // Arrange & Act
        var list = new InlineList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
        Assert.Equal(3, list[2]);
    }

    [Fact]
    public void WorksWithNullableValueTypes()
    {
        // Arrange
        var list = new InlineList<int?>();

        // Act
        list.Add(1);
        list.Add(null);
        list.Add(3);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Null(list[1]);
        Assert.Equal(3, list[2]);
    }

    #endregion
}
