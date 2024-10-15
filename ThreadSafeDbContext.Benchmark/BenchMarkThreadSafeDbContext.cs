using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class BenchMarkThreadSafeDbContext : Microsoft.EntityFrameworkCore.ThreadSafe.ThreadSafeDbContext
{
    public BenchMarkThreadSafeDbContext(DbContextOptions<BenchMarkThreadSafeDbContext> options) : base(
        options)
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