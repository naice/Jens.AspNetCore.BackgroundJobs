using System.ComponentModel.DataAnnotations;

namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// The <see cref="Job"/> model.
/// </summary>
public class Job
{
    /// <summary>
    /// The Identifier.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
    /// <summary>
    /// This is the type name of the job task to launch when executing this job.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Status of the job.
    /// </summary>
    public JobStatus Status { get; set; }
    /// <summary>
    /// [Optional] message.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// [Optional] paramter.
    /// </summary>
    public string? Parameter { get; set; }
    /// <summary>
    /// [Optional] finished timestamp.
    /// </summary>
    public DateTime? Finished { get; set; }
    /// <summary>
    /// The begin timestamp. The Job wants to be started here, but this can not be guaranteed.
    /// </summary>
    public DateTime Begin { get; set; }
    /// <summary>
    /// [Optional] the actual begin timestamp.
    /// </summary>
    /// <value></value>
    public DateTime? ActualBegin { get; set; }
}
