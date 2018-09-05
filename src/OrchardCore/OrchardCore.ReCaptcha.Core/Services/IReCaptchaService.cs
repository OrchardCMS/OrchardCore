using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.ReCaptcha.Core.Services
{
    public interface IReCaptchaService
    {
        /// <summary>
        /// Verifies the ReCaptcha response with the ReCaptcha webservice
        /// </summary>
        /// <param name="reCaptchaResponse"></param>
        /// <returns></returns>
        Task<bool> VerifyCaptchaResponseAsync(string reCaptchaResponse);

        /// <summary>
        /// Clears all convictions and resets all flags
        /// </summary>
        /// <returns></returns>
        Task MarkAsInnocentAsync();

        /// <summary>
        /// Flags the behavior of the request as suspect, internal implementations will decide if the behavior is convicted 
        /// </summary>
        /// <returns></returns>
        Task FlagAsSuspectAsync();

        /// <summary>
        /// Determines if the request has been convicted as abuse
        /// </summary>
        /// <returns></returns>
        Task<bool> IsConvictedAsync();
    }
}
