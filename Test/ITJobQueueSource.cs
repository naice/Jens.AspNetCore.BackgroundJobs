using Jens.AspNetCore.BackgroundJobs;

namespace Test;

public class ITJobQueueSource : IJobQueueSource
{
    public Queue<Job> Queue { get; set; } = new Queue<Job>(new [] {
        new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED },
        new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED },
        new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED },
        new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED },
    });

    public List<Job> UpdatedJobs { get; set; } = new List<Job>();


    public Task<Job?> GetNextJob(Type jobTaskType)
    {
        return Task.FromResult<Job?>(Queue.Dequeue());
    }

    public Task UpdateJob(Type jobTaskType, Job job)
    {
        UpdatedJobs.Add(job.JsonClone());
        return Task.CompletedTask;
    }
}
