using System.Text.Json;
using Jens.AspNetCore.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Test;

public class JobSpawnServiceTest
{
    private (
        JobSpawnService<MockJobTask> service,
        MockJobTask mockJobTask,
        List<Job> jobsUpdated,
        Mock<IServiceProvider> serviceProvider,
        Mock<IJobQueueSource> jobQueueSource,
        Mock<ILogger<JobSpawnService<MockJobTask>>> logger
    ) 
    CreateWiredService(Action<MockJobTask>? configure = null)
    {
        var jobsUpdated = new List<Job>();
        var mockJobTask = new MockJobTask();
        configure?.Invoke(mockJobTask);
        Mock<ILogger<JobSpawnService<MockJobTask>>> logger = new Mock<ILogger<JobSpawnService<MockJobTask>>>();
        Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
        Mock<IServiceScope> serviceScope = new Mock<IServiceScope>();
        Mock<IJobQueueSource> jobQueueSource = new Mock<IJobQueueSource>();
        jobQueueSource.Setup(x => x.UpdateJob(It.IsAny<Job>())).Callback<Job>(x => {
            jobsUpdated.Add(x.JsonClone());
        });
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(IJobQueueSource))).Returns(jobQueueSource.Object);
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(MockJobTask))).Returns(mockJobTask);
        Mock<IServiceScopeFactory> serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);

        var service = new JobSpawnService<MockJobTask>(
            serviceProvider.Object, 
            new JobSpawnServiceConfiguration<MockJobTask>() { NextJobQueryDelay = TimeSpan.FromSeconds(0.1) },
            logger.Object
        );

        return (service, mockJobTask, jobsUpdated, serviceProvider, jobQueueSource, logger);
    }

    [Fact]
    public async void ShouldErrorIf_GetNextJob_Throws()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        var wired = CreateWiredService();
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ThrowsAsync(new Exception(EXCEPTION_ID));
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);

        wired.logger.VerifyLogEquals(
            LogLevel.Information,
            "Job Spawn Service starting.");

        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            $"Unhandled Exception in {nameof(IJobQueueSource)}.{nameof(IJobQueueSource.GetNextJob)}",
            EXCEPTION_ID);
    }

    [Fact]
    public async void Should_Continue_On_Null_Jobs()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        var wired = CreateWiredService();
        var calls = 0;
        var job = new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED };
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ReturnsAsync(
            () => {
                if (calls == 0) { calls++; return null; }
                else if (calls == 1) { calls++; return job; }
                else throw new Exception(EXCEPTION_ID);
            }
        );
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);

        job.Status.Should().Be(JobStatus.FINISHED);

        // ensure exits like expected.
        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            EXCEPTION_ID);        
    }

    [Fact]
    public async void Should_Continue_On_Delayed_Jobs()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        var wired = CreateWiredService((x) => x.WithResult(new JobTaskResult(JobStatus.ERROR)));
        var calls = 0;
        var continueJob = new Job() { Begin = DateTime.MaxValue, Status = JobStatus.CREATED };
        var job = new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED };
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ReturnsAsync(
            () => {
                if (calls == 0) { calls++; return continueJob; }
                else if (calls == 1) { calls++; return job; }
                else throw new Exception(EXCEPTION_ID);
            }
        );
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);

        continueJob.Status.Should().Be(JobStatus.CREATED);
        job.Status.Should().Be(JobStatus.ERROR, "The result state should reach here.");

        // ensure exits like expected.
        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            EXCEPTION_ID);        
    }

    [Fact]
    public async void Ensure_Required_UpdateJob_Calls()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        string resultMessage = nameof(resultMessage);
        var wired = CreateWiredService((x) => x.WithResult(new JobTaskResult(JobStatus.FINISHED, resultMessage)));
        var calls = 0;
        var job = new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED };
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ReturnsAsync(
            () => {
                if (calls == 0) { calls++; return job; }
                else throw new Exception(EXCEPTION_ID);
            }
        );
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);

        var progressJobs = wired.jobsUpdated.Where(x => x.Status == JobStatus.PROGRESS);
        progressJobs.Should().ContainSingle();
        
        var finishedJobs = wired.jobsUpdated.Where(x => x.Status == JobStatus.FINISHED);
        finishedJobs.Should().ContainSingle();
        
        job.Finished.Should().NotBeNull();
        job.Message.Should().Be(resultMessage, "Because the job result message should be set to the Job.");

        // ensure exits like expected.
        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            EXCEPTION_ID);        
    }

    [Fact]
    public async void Ensure_Exception_Job_Status()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        string JOB_EXCEPTION_ID = Guid.NewGuid().ToString();
        var wired = CreateWiredService((x) => x.WithException(new Exception(JOB_EXCEPTION_ID)));
        var calls = 0;
        var job = new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED };
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ReturnsAsync(
            () => {
                if (calls == 0) { calls++; return job; }
                else throw new Exception(EXCEPTION_ID);
            }
        );
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);
        
        job.Status.Should().Be(JobStatus.ERROR);
        job.Message.Should().StartWith("The job task execution failed.");
        job.Finished.Should().BeNull();

        // ensure exits like expected.
        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            EXCEPTION_ID);        
    }

    [Fact]
    public async void Ensure_UpdateJob_Exception_Is_Caught_And_Execution_Stops()
    {
        string EXCEPTION_ID = Guid.NewGuid().ToString();
        var wired = CreateWiredService();
        var job = new Job() { Begin = DateTime.MinValue, Status = JobStatus.CREATED };
        wired.jobQueueSource.Reset();
        wired.jobQueueSource.Setup(x => x.GetNextJob(typeof(MockJobTask))).ReturnsAsync(job);
        wired.jobQueueSource.Setup(x => x.UpdateJob(It.IsAny<Job>())).ThrowsAsync(new Exception(EXCEPTION_ID));
        await wired.service.ExecuteAsyncPublic(CancellationToken.None);
        
        job.ActualBegin.Should().NotBeNull("job execution was about to start.");
        job.Status.Should().Be(JobStatus.PROGRESS, $"the job should stop on first update attempt which is {JobStatus.PROGRESS}.");
        
        // ensure the exception thrown was at least logged.
        wired.logger.VerifyLogContainsAll(
            LogLevel.Error,
            EXCEPTION_ID);        
    }
}