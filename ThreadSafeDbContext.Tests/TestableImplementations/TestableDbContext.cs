namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class TestableDbContext : ThreadSafeDbContext
{
    public TestableDbContext(DbContextOptionsBuilder<TestableDbContext> optionsBuilder) : base(optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestableEntity>().ToTable("TestableEntity");
        modelBuilder.Entity<TestableEntity>().HasKey(t => t.ID);
        modelBuilder.Entity<TestableEntity>().HasMany<TestableEntityDependency>(t => t.Dependencies).WithOne()
            .HasForeignKey(t => t.TestableEntityID);
        modelBuilder.Entity<TestableEntity>().Property(t => t.Name);


        modelBuilder.Entity<TestableEntityDependency>().ToTable("TestableEntityDependency");
        modelBuilder.Entity<TestableEntityDependency>().HasKey(t => t.ID);
        modelBuilder.Entity<TestableEntityDependency>().Property(t => t.Name);

        base.OnModelCreating(modelBuilder);
    }
}