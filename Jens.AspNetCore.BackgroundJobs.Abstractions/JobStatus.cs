namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// The <see cref="Job"/> status.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Error, the job won't be started again.
    /// </summary>
    ERROR = 0,
    /// <summary>
    /// Created, the job is able to start.
    /// </summary>
    CREATED = 1,
    /// <summary>
    /// Progress, the job is beeing executed.1
    /// </summary>
    PROGRESS = 2,
    /// <summary>
    /// Finished, the job won't be started again.
    /// </summary>
    FINISHED = 3
}