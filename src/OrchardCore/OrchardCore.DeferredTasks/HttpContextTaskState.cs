using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.DeferredTasks
{
    /// <summary>
    /// Maintains a list of tasks by using the active <see cref="HttpContext.Items"/> property
    /// as storage.
    /// </summary>
    public class HttpContextTaskState : IDeferredTaskState
    {
        private readonly static object Key = typeof(HttpContextTaskState);
        private readonly HttpContext _httpContext;

        public HttpContextTaskState(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }

        public IList<Func<DeferredTaskContext, Task>> Tasks
        {
            get
            {
                object tasks;

                if (!_httpContext.Items.TryGetValue(Key, out tasks))
                {
                    tasks = new List<Func<DeferredTaskContext, Task>>();
                    _httpContext.Items[Key] = tasks;
                }

                return (IList<Func<DeferredTaskContext, Task>>)tasks;
            }
        }
    }
}
