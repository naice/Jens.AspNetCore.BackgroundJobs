using Microsoft.Extensions.DependencyInjection;

namespace Jens.AspNetCore.BackgroundJobs;

/// <summary>
/// <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IJobQueueSource"/> scoped.
    /// Please visit http://www.github.com/naice/Jens.AspNetCore.BackgroundJobs for more informations on how to use <see cref="Jens.AspNetCore.BackgroundJobs"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <typeparam name="TJobQueueSource">The implementation of the <see cref="IJobQueueSource"/> to register.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> for fluent syntax.</returns>
    public static IServiceCollection AddJobQueueSource<TJobQueueSource>(this IServiceCollection services)
        where TJobQueueSource : class, IJobQueueSource
        => services.AddScoped<IJobQueueSource, TJobQueueSource>();

    /// <summary>
    /// Registers the implementation of your <see cref="IJobTask"/>,
    /// the associated <see cref="JobSpawnService{TJobTask}"/>
    /// and its configuration (<see cref="JobSpawnServiceConfiguration{TJobTask}"/>).
    /// The spawn service will auto launch and request jobs.
    /// Please visit http://www.github.com/naice/Jens.AspNetCore.BackgroundJobs for more informations on how to use <see cref="Jens.AspNetCore.BackgroundJobs"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">The configuration callback.</param>
    /// <typeparam name="TJobTask">The implementation of the <see cref="TJobTask"/> to register.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> for fluent syntax.</returns>
    public static IServiceCollection AddJobTask<TJobTask>(this IServiceCollection services, Action<JobSpawnServiceConfiguration<TJobTask>>? configure = null)
        where TJobTask : class, IJobTask
    {
        // add config
        var configuration = new JobSpawnServiceConfiguration<TJobTask>();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);
        
        // add job task
        services.AddScoped<TJobTask>();

        // add job spawn service for given task.
        services.AddHostedService<JobSpawnService<TJobTask>>();
        return services;
    }
}