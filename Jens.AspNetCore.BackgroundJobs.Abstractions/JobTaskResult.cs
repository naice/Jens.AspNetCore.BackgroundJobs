namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// The <see cref="JobTaskResult"/>.
/// </summary>
/// <param name="Status">Affects the lifecycle of the associated <see cref="Job"/>.</param>
/// <param name="Message">[Optional] Message that contains detailed information about the job state.</param>
public record JobTaskResult(JobStatus Status, string? Message = null);
