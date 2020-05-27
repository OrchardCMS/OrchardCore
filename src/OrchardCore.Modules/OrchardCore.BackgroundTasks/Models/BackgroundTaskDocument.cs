/*
	background task data model
*/

using System.Collections.Generic;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
    	// set of relations 'string (~name) : settings'
        public Dictionary<string, BackgroundTaskSettings> Settings { get; } = new Dictionary<string, BackgroundTaskSettings>();
    }
}
