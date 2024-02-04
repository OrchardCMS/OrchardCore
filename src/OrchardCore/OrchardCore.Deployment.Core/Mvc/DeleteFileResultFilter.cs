using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrchardCore.Deployment.Core.Mvc
{
    public class DeleteFileResultFilter : ResultFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.Result is not PhysicalFileResult result)
            {
                return;
            }

            var fileInfo = new FileInfo(result.FileName);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }
    }
}
