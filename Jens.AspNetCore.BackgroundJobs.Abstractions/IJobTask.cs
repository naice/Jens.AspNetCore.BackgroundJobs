namespace Jens.AspNetCore.BackgroundJobs;


/// <summary>
/// The <see cref="IJobTask"/> interface.
/// </summary>
public interface IJobTask
{
    /// <summary>
    /// Runs the given <see cref="Job"/>. All exceptions are catched and stored to <see cref="Job.Message"/> as well as an status change.
    /// </summary>
    /// <param name="job">The <see cref="Job"/> to run.</param>
    /// <returns>
    /// A <see cref="JobTaskResult"/> keep in mind that the provided result affects the job lifecycle.
    /// </returns>
    Task<JobTaskResult> Run(Job job);
}
