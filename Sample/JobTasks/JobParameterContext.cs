using System.Text.Json;
using Jens.AspNetCore.BackgroundJobs;

namespace Sample.JobTasks;

// I like to use this generic context for the job execution
public class JobParameterContext<T>
{
    public T Parameter { get; set; }
    public Job Job { get; }    
    private readonly IJobQueueSource _jobSource;

    public JobParameterContext(T parameter, Job job, IJobQueueSource jobSource)
    {
        Parameter = parameter;
        Job = job;
        _jobSource = jobSource;
    }

    public async Task Update(Action<T>? a = null)
    {
        a?.Invoke(Parameter);
        Job.Parameter = JsonSerializer.Serialize(Parameter);
        await _jobSource.UpdateJob(Job);
    }
}
