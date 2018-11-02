using System.Text;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
	/// An <see cref="IPredicate"/> that constrains the property to a specified list of values.
	/// </summary>
	public class InExpression : IPredicate
	{
		private readonly string _propertyName;

	    public InExpression(string propertyName, object[] values)
		{
			_propertyName = propertyName;
			Values = values;
		}

	    public object[] Values { get; protected set; }

	    public string ToSqlString(IPredicateQuery predicateQuery)
	    {
			if (Values.Length == 0)
			{
				// 'columnName in ()' is always false
				return "1=0";
			}

			// Generates:
			//  columnName in (@p1, @p2, @p3)

	        var array = new StringBuilder();
	        for (int i = 0; i < Values.Length; i++)
	        {
	            var parameter = predicateQuery.NewQueryParameter(Values[i]);

	            if (i > 0)
	            {
	                array.Append(", ");
	            }
	            array.Append(parameter);
	        }

            // TODO: Column Name
	        var columnName = "";
	        return $"{columnName} in ({array})";
		}
	}
}