using Jens.AspNetCore.BackgroundJobs;

namespace Test;

public class MockJobTask : IJobTask
{
    private JobTaskResult? _result;
    private Exception? _exception;

    public Job? Job { get; set; } = null;
    public CancellationToken CancellationToken { get; set; }

    public MockJobTask WithResult(JobTaskResult result) 
    {
        _result = result;
        return this;
    }
    public MockJobTask WithException(Exception ex)
    {
        _exception = ex;
        return this;
    }

    public Task<JobTaskResult> Run(Job job)
    {
        Job = job;
        if (_exception != null)
            throw _exception;
        
        return Task.FromResult(_result ?? new JobTaskResult(JobStatus.FINISHED, job.Parameter));
    }
}
