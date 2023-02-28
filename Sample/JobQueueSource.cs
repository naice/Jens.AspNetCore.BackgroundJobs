using Jens.AspNetCore.BackgroundJobs;

namespace Sample;
public class JobQueueSource : IJobQueueSource
{
    private readonly InMemoryDbContext _db;

    public JobQueueSource(InMemoryDbContext db)
    {
        _db = db;
    }
    
    public Task<Job?> GetNextJob(Type jobTaskType)
    {
        var job = _db.Jobs
            .Where(x => x.Status == JobStatus.CREATED && x.TypeName == jobTaskType.FullName)
            .OrderBy(x=> x.Begin)
            .FirstOrDefault();

        return Task.FromResult<Job?>(job);
    }

    public async Task UpdateJob(Job job)
    {
        _db.Update(job);
        await _db.SaveChangesAsync();
    }
}
