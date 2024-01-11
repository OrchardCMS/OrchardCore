using System;

namespace OrchardCore.Queries.Sql
{
    public class SqlParserException : Exception
    {
        public SqlParserException(string message) : base(message)
        {
        }
    }
}
