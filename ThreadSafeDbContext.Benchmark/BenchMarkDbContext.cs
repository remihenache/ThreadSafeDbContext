using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class BenchMarkDbContext : DbContext
{
    public BenchMarkDbContext(DbContextOptionsBuilder<BenchMarkDbContext> optionsBuilder) : base(optionsBuilder.Options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BenchMarkEntity>().ToTable("TestableEntity");
        modelBuilder.Entity<BenchMarkEntity>().HasKey(t => t.ID);
        modelBuilder.Entity<BenchMarkEntity>().Property(t => t.Name);


        base.OnModelCreating(modelBuilder);
    }
}