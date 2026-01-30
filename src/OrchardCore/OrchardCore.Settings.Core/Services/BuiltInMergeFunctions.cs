namespace OrchardCore.Settings.Services;

/// <summary>
/// Merges arrays by combining unique values from both sources.
/// File values take precedence for duplicate entries.
/// </summary>
public class ArrayMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        if (!context.PropertyType.IsArray)
        {
            return fileValue ?? databaseValue;
        }

        var elementType = context.PropertyType.GetElementType();

        var dbArray = databaseValue as Array;
        var fileArray = fileValue as Array;

        // If one is null, return the other
        if (dbArray == null && fileArray == null)
        {
            return Array.CreateInstance(elementType, 0);
        }

        if (dbArray == null)
        {
            return fileArray;
        }

        if (fileArray == null)
        {
            return dbArray;
        }

        // Combine unique values
        var combined = new HashSet<object>();

        // Add file values first (they take precedence)
        foreach (var item in fileArray)
        {
            combined.Add(item);
        }

        // Add unique database values
        foreach (var item in dbArray)
        {
            combined.Add(item);
        }

        var result = Array.CreateInstance(elementType, combined.Count);
        var index = 0;
        foreach (var item in combined)
        {
            result.SetValue(item, index++);
        }

        return result;
    }
}

/// <summary>
/// Merges numeric values by taking the maximum of both.
/// </summary>
public class MaxValueMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        var dbValue = Convert.ToDouble(databaseValue ?? 0);
        var filValue = Convert.ToDouble(fileValue ?? 0);

        var maxValue = Math.Max(dbValue, filValue);

        // Convert back to the original type
        return Convert.ChangeType(maxValue, context.PropertyType);
    }
}

/// <summary>
/// Merges numeric values by taking the minimum of both.
/// </summary>
public class MinValueMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        if (databaseValue == null && fileValue == null)
        {
            return Activator.CreateInstance(context.PropertyType);
        }

        if (databaseValue == null)
        {
            return fileValue;
        }

        if (fileValue == null)
        {
            return databaseValue;
        }

        var dbValue = Convert.ToDouble(databaseValue);
        var filValue = Convert.ToDouble(fileValue);

        var minValue = Math.Min(dbValue, filValue);

        // Convert back to the original type
        return Convert.ChangeType(minValue, context.PropertyType);
    }
}

/// <summary>
/// Merges dictionaries by combining key-value pairs.
/// File values take precedence for duplicate keys.
/// </summary>
public class DictionaryMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        if (!context.PropertyType.IsGenericType)
        {
            return fileValue ?? databaseValue;
        }

        var genericDef = context.PropertyType.GetGenericTypeDefinition();
        if (genericDef != typeof(Dictionary<,>) && genericDef != typeof(IDictionary<,>))
        {
            return fileValue ?? databaseValue;
        }

        var keyType = context.PropertyType.GetGenericArguments()[0];
        var valueType = context.PropertyType.GetGenericArguments()[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

        var result = (System.Collections.IDictionary)Activator.CreateInstance(dictType);

        // Add database values first
        if (databaseValue is System.Collections.IDictionary dbDict)
        {
            foreach (System.Collections.DictionaryEntry entry in dbDict)
            {
                result[entry.Key] = entry.Value;
            }
        }

        // File values override database values (added second)
        if (fileValue is System.Collections.IDictionary fileDict)
        {
            foreach (System.Collections.DictionaryEntry entry in fileDict)
            {
                result[entry.Key] = entry.Value;
            }
        }

        return result;
    }
}

/// <summary>
/// Merges boolean values using OR logic (either true results in true).
/// Useful for feature flags where file can enable but not disable.
/// </summary>
public class BooleanOrMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        var dbBool = databaseValue as bool? ?? false;
        var fileBool = fileValue as bool? ?? false;

        return dbBool || fileBool;
    }
}

/// <summary>
/// Merges boolean values using AND logic (both must be true for result to be true).
/// Useful for feature flags where file can disable but only UI can enable.
/// </summary>
public class BooleanAndMergeFunction : IPropertyMergeFunction
{
    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        var dbBool = databaseValue as bool? ?? true;
        var fileBool = fileValue as bool? ?? true;

        return dbBool && fileBool;
    }
}

/// <summary>
/// Merges string values by concatenating with a separator.
/// </summary>
public class StringConcatMergeFunction : IPropertyMergeFunction
{
    /// <summary>
    /// Gets or sets the separator to use when concatenating strings.
    /// Defaults to System.Environment.NewLine.
    /// </summary>
    public string Separator { get; set; } = System.Environment.NewLine;

    /// <inheritdoc/>
    public object Merge(object databaseValue, object fileValue, PropertyMergeContext context)
    {
        var dbStr = databaseValue as string ?? string.Empty;
        var fileStr = fileValue as string ?? string.Empty;

        if (string.IsNullOrEmpty(dbStr))
        {
            return fileStr;
        }

        if (string.IsNullOrEmpty(fileStr))
        {
            return dbStr;
        }

        return $"{fileStr}{Separator}{dbStr}";
    }
}
