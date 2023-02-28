using Jens.AspNetCore.BackgroundJobs;
using Microsoft.EntityFrameworkCore;

namespace Sample;

public class InMemoryDbContext : DbContext
{
    public DbSet<Job> Jobs { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    {   
        optionsBuilder.UseInMemoryDatabase(nameof(InMemoryDbContext));
    }
     
}
