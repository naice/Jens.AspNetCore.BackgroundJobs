using Jens.AspNetCore.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test;

public class IntegrationTest
{
    [Fact]
    public void Ensure_JobSpawnService_Creation()
    {
        var mockLoggerProvider = new Mock<ILoggerProvider>();
        mockLoggerProvider.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns<string>(c =>
        {
            return NullLoggerProvider.Instance.CreateLogger(c);
        });
        var col = new ServiceCollection();
        col.AddLogging(l => l.ClearProviders().AddProvider(mockLoggerProvider.Object));
        col.AddJobQueueSource<ITJobQueueSource>();
        col.AddJobTask<ITJobTask>(cfg => cfg.NextJobQueryDelay = TimeSpan.MinValue);
        var sp = col.BuildServiceProvider();
        var service = sp.GetService<IHostedService>();
        service.Should().NotBeNull();
    }
}
