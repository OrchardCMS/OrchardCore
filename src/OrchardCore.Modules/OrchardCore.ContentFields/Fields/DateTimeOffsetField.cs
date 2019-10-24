using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class DateTimeOffsetField : ContentField
    {
        public DateTime? Value { get; set; }

        /// <summary>
        /// An offset from UTC in seconds.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// 2-letter country code
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// NodaTime TimeZone Id
        /// </summary>
        public string TimeZoneId { get; set; }
    }
}
