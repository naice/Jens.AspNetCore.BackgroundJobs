using Jens.AspNetCore.BackgroundJobs;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

// Generic 
public abstract class GenericJobTaskController<TJobTask, CreateRequest> : ControllerBase
{
    private readonly ILogger<GenericJobTaskController<TJobTask, CreateRequest>> _logger;
    private readonly InMemoryDbContext _context;

    private readonly string _jobTaskTypeName = typeof(TJobTask).FullName!;

    public GenericJobTaskController(
        ILogger<GenericJobTaskController<TJobTask, CreateRequest>> logger,
        InMemoryDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    protected abstract string? CreateParameter(CreateRequest request);
    
    [HttpPost(nameof(CreateJob))]
    public async Task<Job> CreateJob(CreateRequest request)
    {
        var job = new Job();
        job.Begin = DateTime.UtcNow;
        job.Created = DateTime.UtcNow;
        job.Id = Guid.NewGuid();
        job.Status = JobStatus.CREATED;
        job.TypeName = _jobTaskTypeName;     
        job.Parameter = CreateParameter(request);
        
        _context.Add(job);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created job.");
        return job;
    }

    
    [HttpPost(nameof(ListRecent))]
    public Task<Job[]> ListRecent()
    {
        return Task.FromResult(
            _context.Jobs.Where(x => 
                    x.TypeName == _jobTaskTypeName && 
                    x.Begin > DateTime.UtcNow - TimeSpan.FromDays(7))
                .OrderBy(x=> x.Begin)
                .ToArray()
        );
    }
}
