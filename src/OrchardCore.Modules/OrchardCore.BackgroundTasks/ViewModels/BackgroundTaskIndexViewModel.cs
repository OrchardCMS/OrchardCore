/*
	The data model (~) of the 'background tasks' list
	and a pager (~ something like optimizing memory usage) for them
*/

using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.ViewModels
{
    public class BackgroundTaskIndexViewModel
    {
    	// tasks - list of 'background task settings'
        public IList<BackgroundTaskEntry> Tasks { get; set; }
        // (~) implements 'swap memory'
        public dynamic Pager { get; set; }
    }

    public class BackgroundTaskEntry
    {
        public BackgroundTaskSettings Settings { get; set; }
    }
}
