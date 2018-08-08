using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.ReCaptcha.Core.ActionFilters.Abuse
{
    public interface IDetectAbuse
    {
        AbuseDetectResult DetectAbuse(HttpContext context);

        void ClearAbuseFlags(HttpContext context);

        void FlagPossibleAbuse(HttpContext context);
    }

    public class AbuseDetectResult
    {
        public bool SuspectAbuse { get; set; }
    }
}
