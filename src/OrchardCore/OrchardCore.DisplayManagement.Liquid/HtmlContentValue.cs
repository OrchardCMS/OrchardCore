using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class HtmlContentValue : FluidValue
    {
        private readonly IHtmlContent _value;

        public HtmlContentValue(IHtmlContent value)
        {
            _value = value;
        }

        public override FluidValues Type => FluidValues.String;

        public override bool Equals(FluidValue other)
        {
            if (other.IsNil())
            {
                return _value == null;
            }

            return _value == other;
        }

        protected override FluidValue GetIndex(FluidValue index, TemplateContext context)
        {
            return NilValue.Instance;
        }

        protected override FluidValue GetValue(string name, TemplateContext context)
        {
            return NilValue.Instance;
        }

        public override bool ToBooleanValue()
        {
            return true;
        }

        public override decimal ToNumberValue()
        {
            return 0;
        }

        public override string ToStringValue()
        {
            return _value.ToString();
        }

        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        {
            _value.WriteTo(writer, (HtmlEncoder)encoder);
        }

        public override object ToObjectValue()
        {
            return _value;
        }

        public override bool Contains(FluidValue value)
        {
            return false;
        }

        public override IEnumerable<FluidValue> Enumerate(TemplateContext context)
        {
            return Enumerable.Empty<FluidValue>();
        }

        public override bool Equals(object other)
        {
            // The is operator will return false if null
            if (other is HtmlContentValue otherValue)
            {
                return _value.Equals(otherValue._value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
