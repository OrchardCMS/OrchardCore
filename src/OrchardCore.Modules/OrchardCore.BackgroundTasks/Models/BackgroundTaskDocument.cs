using System.Collections.Immutable;

namespace OrchardCore.BackgroundTasks.Models
{
    public class BackgroundTaskDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public ImmutableDictionary<string, BackgroundTaskSettings> Settings { get; set; } = ImmutableDictionary.Create<string, BackgroundTaskSettings>();
    }
}
