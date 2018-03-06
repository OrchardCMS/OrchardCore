using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Queries.Liquid
{
    public class QueryValue : FluidValue
    {
        private readonly Lazy<FluidValue> _queryResult;

        public override FluidValues Type => FluidValues.Array;
        public IDictionary<string, object> Parameters = new Dictionary<string, object>();

        public QueryValue(IQueryManager queryManager, string queryName)
        {
            _queryResult = new Lazy<FluidValue>(() =>
            {
                var query = queryManager.GetQueryAsync(queryName).GetAwaiter().GetResult();

                if (query == null)
                {
                    return null;
                }

                var result = queryManager.ExecuteQueryAsync(query, Parameters).GetAwaiter().GetResult();

                return FluidValue.Create(result);
            });
        }

        public override bool Equals(FluidValue other)
        {
            return _queryResult.Value.Equals(other);
        }

        public override bool ToBooleanValue()
        {
            return _queryResult.Value.ToBooleanValue();
        }

        public override double ToNumberValue()
        {
            return _queryResult.Value.ToNumberValue();
        }

        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        {
            encoder.Encode(writer, ToStringValue());
        }

        public override string ToStringValue()
        {
            return _queryResult.Value.ToStringValue();
        }

        public override object ToObjectValue()
        {
            return _queryResult.Value.ToObjectValue();
        }

        public override Task<FluidValue> GetValueAsync(string name, TemplateContext context)
        {
            return _queryResult.Value.GetValueAsync(name, context);
        }

        public override Task<FluidValue> GetIndexAsync(FluidValue index, TemplateContext context)
        {
            return _queryResult.Value.GetIndexAsync(index, context);
        }

        public override bool Contains(FluidValue value)
        {
            return _queryResult.Value.Contains(value);
        }

        public override IEnumerable<FluidValue> Enumerate()
        {
            return _queryResult.Value.Enumerate();
        }
    }
}