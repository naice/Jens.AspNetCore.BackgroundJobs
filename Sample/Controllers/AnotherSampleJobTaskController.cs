using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Sample.JobTasks;

namespace Sample.Controllers;

[ApiController]
[Route("[controller]")]
public class AnotherSampleJobTaskController : GenericJobTaskController<AnotherSampleJobTask, CreateAnotherSampleJobTask>
{
    public AnotherSampleJobTaskController(ILogger<GenericJobTaskController<AnotherSampleJobTask, CreateAnotherSampleJobTask>> logger, InMemoryDbContext context) : base(logger, context)
    {
    }

    protected override string? CreateParameter(CreateAnotherSampleJobTask request)
        => JsonSerializer.Serialize(request);
}
