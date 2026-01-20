using YesSql;

namespace OrchardCore.Queries.Sql;

public class SqlParser
{
    public static bool TryParse(string sql, string schema, ISqlDialect dialect, string tablePrefix, IDictionary<string, object> parameters, out string query, out IEnumerable<string> messages)
    {
        try
        {
            // Parse using Parlot
            if (!ParlotSqlParser.TryParse(sql, out var statementList, out var error))
            {
                query = null;
                messages = error != null 
                    ? new string[] { $"Parse error: {error.Message} at position {error.Position}" }
                    : new string[] { "Parse error: Unknown parsing error" };
                return false;
            }

            // Translate the AST to SQL
            var translator = new SqlTranslator(schema, dialect, tablePrefix, parameters);
            query = translator.Translate(statementList);

            messages = Array.Empty<string>();
            return true;
        }
        catch (SqlParserException se)
        {
            query = null;
            messages = new string[] { se.Message };
        }
        catch (Exception e)
        {
            query = null;
            messages = new string[] { "Unexpected error: " + e.Message };
        }

        return false;
    }
}
