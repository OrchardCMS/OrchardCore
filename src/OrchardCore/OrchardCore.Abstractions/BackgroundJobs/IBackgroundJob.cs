namespace OrchardCore.BackgroundJobs
{ 
    public interface IBackgroundJob
    {
        /// <summary>
        /// The name of the background job.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The id of the background job.
        /// </summary>
        string BackgroundJobId { get; set; }

        /// <summary>
        /// When a <see cref="IBackgroundJob"/> is repeated this refers to the first id that initiated the schedule.
        /// </summary>
        string RepeatCorrelationId { get; set; }

        /// <summary>
        /// An id which correlates to another type of document.
        /// </summary>
        string CorrelationId { get; set; }
    }
}
