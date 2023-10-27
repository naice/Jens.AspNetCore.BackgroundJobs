using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// Typed <see cref="JobSpawnService"/> configuration.
/// </summary>
/// <typeparam name="TJobTask">The <see cref="TJobTask" type this config is associated to.</typeparam>
public class JobSpawnServiceConfiguration<TJobTask>
{
    /// <summary>
    /// The delay that is await between job task executions.
    /// </summary>
    /// <returns></returns>
    public TimeSpan NextJobQueryDelay { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// The spawning service for the given <see cref="TJobTask"/>.
/// </summary>
/// <typeparam name="TJobTask">Type of the job task this spawning service is associated to.</typeparam>
public class JobSpawnService<TJobTask> : BackgroundService 
    where TJobTask : IJobTask
{
    private readonly ILogger<JobSpawnService<TJobTask>> _logger;
    private readonly Type _jobTaskType;
    private readonly IServiceProvider _services;
    private readonly JobSpawnServiceConfiguration<TJobTask> _config;

    /// <summary>
    /// Creates an instance of the <see cref="JobSpawnService{TJobTask}"/> type.
    /// </summary>
    /// <param name="services"><see cref="Microsoft.Extensions.DependencyInjection"> for more information on dependency injection.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public JobSpawnService(
        IServiceProvider services,
        JobSpawnServiceConfiguration<TJobTask> config,
        ILogger<JobSpawnService<TJobTask>> logger)
    {
        _jobTaskType = typeof(TJobTask);
        _services = services;
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// For easier testing. 
    /// </summary>
    public Task ExecuteAsyncPublic(CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Spawn Service starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            // wait for next execution.
            await Task.Delay(_config.NextJobQueryDelay, cancellationToken);

            // create a scope and get the queue source
            using var scope = _services.CreateScope();
            var jobQueueSource = scope.ServiceProvider.GetRequiredService<IJobQueueSource>();

            // get next job
            Job? job = null;
            try
            {
                job = await jobQueueSource.GetNextJob(_jobTaskType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled Exception in {nameof(IJobQueueSource)}.{nameof(IJobQueueSource.GetNextJob)}. JobSpawnService shutting down. InnerMessage=" + ex.Message);
                break;
            }

            if (job == null)
            {
                // nothing to do
                continue;
            }

            if (DateTime.UtcNow < job.Begin)
            {
                // job can't be started jet.
                continue;
            }

            // get the job task
            var jobTask = (IJobTask)scope.ServiceProvider.GetRequiredService(_jobTaskType);
            jobTask.CancellationToken = cancellationToken;

            // update actual begin time of job
            var jobBegin = DateTime.UtcNow;
            job.ActualBegin = jobBegin;
            job.Status = JobStatus.PROGRESS;
            if (!await UpdateJob(job, jobQueueSource)) break;

            // run the job.
            JobTaskResult result;
            try
            {
                result = await jobTask.Run(job);
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.ERROR;
                job.Message = $"The job task execution failed. {ex}";
                if (!await UpdateJob(job, jobQueueSource)) break;
                continue;
            }

            // job has finished.
            job.Finished = DateTime.UtcNow;
            job.Status = result.Status;
            job.Message = result.Message;
            if (!await UpdateJob(job, jobQueueSource)) break;

        }

        _logger.LogInformation("Job Spawn Service stopped.");
    }

    private async Task<bool> UpdateJob(Job job, IJobQueueSource jobQueueSource)
    {
        try
        {
            await jobQueueSource.UpdateJob(_jobTaskType, job);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled Exception in {nameof(IJobQueueSource)}.{nameof(IJobQueueSource.UpdateJob)}. JobSpawnService shutting down. Exeption: {ex}");
            return false;
        }
    }
}
