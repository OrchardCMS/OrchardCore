using System.Globalization;
using NodaTime;

namespace OrchardCore.Modules
{
    public class TimeZoneCountry : ITimeZoneCountry
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public TimeZoneCountry(string countryCode, string countryName)
        {
            CountryCode = countryCode;
            CountryName = countryName;
        }

        public override string ToString()
        {
            return $"({CountryCode}) {CountryName}";
        }
    }
}
