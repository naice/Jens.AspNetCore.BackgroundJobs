namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// The <see cref="IJobQueueSource"/> interface.
/// </summary>
public interface IJobQueueSource
{
    /// <summary>
    /// Updates the given <see cref="Job"/> with your data source. 
    /// </summary>
    Task UpdateJob(Type jobTaskType, Job job);
    
    /// <summary>
    /// Gets the next <see cref="Job"/> from your data source, that corresponds to the given job task type.
    /// 
    /// Note: An appropriate matching would look like this.
    /// var typeName = jobTaskType.FullName
    /// var job = _dataSource.Jobs.Where(x => x.Status == JobStatus.CREATED && x.TypeName == typeName).OrderBy(x=> x.Begin).FirstOrDefault();
    /// </summary>
    /// <param name="jobTaskType"><see cref="Type"/> that identifies the the job task.</param>
    /// <returns></returns>
    Task<Job?> GetNextJob(Type jobTaskType);
}
