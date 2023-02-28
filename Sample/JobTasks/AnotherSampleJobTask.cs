using Jens.AspNetCore.BackgroundJobs;

namespace Sample.JobTasks;

public class AnotherSampleJobTask 
    : SampleJobTask
{
    public AnotherSampleJobTask(IJobQueueSource jobSource) 
        : base(jobSource) { }
}
