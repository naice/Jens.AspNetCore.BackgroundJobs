using Jens.AspNetCore.BackgroundJobs;

namespace Test;

public class ITJobTask : IJobTask
{
    public CancellationToken CancellationToken { get; set; }
    public Task<JobTaskResult> Run(Job job)
    {
        return Task.FromResult(new JobTaskResult(JobStatus.FINISHED));
    }
}
