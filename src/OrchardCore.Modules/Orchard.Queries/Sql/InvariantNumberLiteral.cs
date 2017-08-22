using System;
using System.Globalization;
using Irony.Parsing;

namespace Orchard.Queries.Sql
{
    public class InvariantNumberLiteral : NumberLiteral
    {
        public InvariantNumberLiteral(string name) : this(name, NumberOptions.Default)
        {
        }
        public InvariantNumberLiteral(string name, NumberOptions options) : base(name)
        {
            Options = options;
            SetFlag(TermFlags.IsLiteral);
        }

        private ExponentsTable _exponentsTable = new ExponentsTable();

        protected override bool ConvertValue(CompoundTokenDetails details, ParsingContext context)
        {
            if (base.ConvertValue(details, context) || details.Error != null)
            {
                return true;
            }

            switch (details.TypeCodes[0])
            {
                case TypeCode.Double:
                    if (QuickConvertToDouble(details)) return true;
                    break;
            }

            return false;
        }

        private bool QuickConvertToDouble(CompoundTokenDetails details)
        {
            if (details.IsSet((short)(NumberOptions.Binary | NumberOptions.Octal | NumberOptions.Hex))) return false;
            if (details.IsSet((short)(NumberFlagsInternal.HasExp))) return false;
            if (DecimalSeparator != '.') return false;
            double dvalue;
            if (!double.TryParse(details.Body, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out dvalue)) return false;
            details.Value = dvalue;
            return true;
        }
    }
}