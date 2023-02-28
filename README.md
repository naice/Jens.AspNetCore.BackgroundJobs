<div align="center">
<img src ="resources/Jens.AspNetCoreBackgroundJobs.svg" alt="Jens.AspNetCoreBackgroundJobs" width="100" height="100">
</div>

# Jens.AspNetCoreBackgroundJobs

![x](https://img.shields.io/badge/License-MIT-blue.svg)

Auto spawning of background tasks based on a Job Queue.

The [Abstractions](/Jens.AspNetCore.BackgroundJobs.Abstractions) library helps you keeping your model libraries free from unwanted dependencies.

# Sample Task Runner

Please also see the [Sample](/Sample) project for more detail.

```csharp
using System.Text.Json;
using Jens.AspNetCore.BackgroundJobs;

namespace Sample.Controllers.JobTasks;

// The Parameter for the job, usually an model known to your API.
public class SampleJobTaskParameter
{
    public int MyProgress { get; set; }
}


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

public class SampleContext : JobParameterContext<SampleJobTaskParameter>
{
    public SampleContext(SampleJobTaskParameter parameter, Job job, IJobQueueSource jobSource) : base(parameter, job, jobSource)
    {
        // i really dont like that overhead here a one line definition should be enough.
    }
}

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
        var rnd = new Random();
        await context.Update(x => x.MyProgress = 10);
        await Task.Delay(1000);
        if (rnd.NextDouble() > 0.5)
            return new JobTaskResult(JobStatus.ERROR, "Sample error");
        await Task.Delay(1000);
        await context.Update(x => x.MyProgress = 20);
        await Task.Delay(1000);
        await context.Update(x => x.MyProgress = 50);
        await Task.Delay(1000);
        await context.Update(x => x.MyProgress = 70);
        await Task.Delay(1000);
        await context.Update(x => x.MyProgress = 100);

        return new JobTaskResult(JobStatus.FINISHED);
    }
}

```
