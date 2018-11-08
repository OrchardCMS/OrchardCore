using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.ReCaptcha.ActionFilters.Abuse
{
    public interface IDetectAbuse
    {
        AbuseDetectResult DetectAbuse();

        void ClearAbuseFlags();

        void FlagPossibleAbuse();
    }

    public class AbuseDetectResult
    {
        public bool SuspectAbuse { get; set; }
    }
}
