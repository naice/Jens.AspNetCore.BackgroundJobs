using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sample.JobTasks;

namespace Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class SampleJobTaskController : GenericJobTaskController<SampleJobTask, CreateSampleJobTask>
{
    public SampleJobTaskController(ILogger<GenericJobTaskController<SampleJobTask, CreateSampleJobTask>> logger, InMemoryDbContext context) : base(logger, context)
    {
    }

    protected override string? CreateParameter(CreateSampleJobTask request)
        => JsonSerializer.Serialize(request);
}
