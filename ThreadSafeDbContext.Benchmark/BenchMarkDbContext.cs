using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class BenchMarkDbContext : DbContext
{
    public BenchMarkDbContext(DbContextOptions<BenchMarkDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BenchMarkEntity>().ToTable("TestableEntity");
        modelBuilder.Entity<BenchMarkEntity>().HasKey(t => t.Id);
        modelBuilder.Entity<BenchMarkEntity>().Property(t => t.Name);


        base.OnModelCreating(modelBuilder);
    }
}