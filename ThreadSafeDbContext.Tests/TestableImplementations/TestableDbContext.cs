using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Tests.TestableImplementations;

public class TestableDbContext : ThreadSafeDbContext
{
    public TestableDbContext(DbContextOptionsBuilder<ThreadSafeDbContext> optionsBuilder) : base(optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestableEntity>().ToTable("TestableEntity");
        modelBuilder.Entity<TestableEntity>().HasKey(t => t.ID);
        base.OnModelCreating(modelBuilder);
    }
}