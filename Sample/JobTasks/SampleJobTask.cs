using System.Text.Json;
using Jens.AspNetCore.BackgroundJobs;

namespace Sample.JobTasks;

public class SampleJobTask : IJobTask
{
    private readonly IJobQueueSource _jobSource;

    public SampleJobTask(IJobQueueSource jobSource)
    {
        _jobSource = jobSource;
    }

    public async Task<JobTaskResult> Run(Job job)
    {
        var param = JsonSerializer.Deserialize<SampleJobTaskParameter>(job.Parameter ?? "null");
        if (param == null) return new JobTaskResult(JobStatus.ERROR, "Empty parameter.");
        var context = new SampleContext(param, job, _jobSource);
        return await SampleBuisnessLogic(context);
    }

    private async Task<JobTaskResult> SampleBuisnessLogic(SampleContext context)
    {
        int initial = context.Parameter.InitialProgress;
        if (initial < 0)
            throw new Exception("A sample exception.");
        if (initial > 100)
            return new JobTaskResult(JobStatus.ERROR, "Sample error.");

        if (initial < 10) 
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 10);
        }
        if (initial < 20)
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 20);
        }
        if (initial < 30)
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 30);
        }
        if (initial < 60)
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 40);
        }
        if (initial < 90)
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 90);
        }
        if (initial < 100)
        {
            await Task.Delay(1000);
            await context.Update(x => x.CurrentProgress = 100);
        }

        return new JobTaskResult(JobStatus.FINISHED, "The sample job is finished.");
    }
}