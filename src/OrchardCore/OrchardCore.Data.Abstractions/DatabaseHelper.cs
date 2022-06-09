using System;

namespace OrchardCore.Data;
public class DatabaseHelper
{
    public static string GetStandardPrefix(string prefix)
    {
        if (String.IsNullOrWhiteSpace(prefix))
        {
            return String.Empty;
        }

        return prefix.Trim() + "_";
    }
}
