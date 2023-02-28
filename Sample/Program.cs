using Jens.AspNetCore.BackgroundJobs;
using Sample;
using Sample.JobTasks;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Add required dependencys for the job tasks.
services.AddScoped<InMemoryDbContext>();
services.AddJobQueueSource<JobQueueSource>();
services.AddJobTask<SampleJobTask>(config => config.NextJobQueryDelay = TimeSpan.FromSeconds(5));
services.AddJobTask<AnotherSampleJobTask>(config => config.NextJobQueryDelay = TimeSpan.FromSeconds(10));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
