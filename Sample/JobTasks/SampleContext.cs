using Jens.AspNetCore.BackgroundJobs;

namespace Sample.JobTasks;

public class SampleContext 
    : JobParameterContext<SampleJobTaskParameter>
{
    public SampleContext(
        SampleJobTaskParameter parameter, 
        Job job, 
        IJobQueueSource jobSource) 
        : base(parameter, job, jobSource) { }
}
