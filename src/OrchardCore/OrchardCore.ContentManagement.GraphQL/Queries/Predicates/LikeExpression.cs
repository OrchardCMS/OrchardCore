using System.Text;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
	/// An <see cref="IPredicate"/> that represents a "like" constraint.
	/// </summary>
	public class LikeExpression : IPredicate
	{
	    private readonly string _propertyName;
	    private readonly string _value;

		public LikeExpression(string propertyName, string value, MatchOptions matchOptions)
			: this(propertyName, matchOptions.ToMatchString(value))
		{
		}

	    public LikeExpression(string propertyName, string value)
	    {
	        _propertyName = propertyName;
	        _value = value;
	    }

	    public string ToSqlString(IPredicateQuery predicateQuery)
		{
			var sql = new StringBuilder();

            // TODO: Column Name
		    sql.Append("TODO: Column Name");
		    sql.Append(" like ").Append(predicateQuery.NewQueryParameter(_value));	
			return sql.ToString();
		}
	}
}