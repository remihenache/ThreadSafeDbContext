namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class NoSafeTestableDbContext : DbContext
{
    public NoSafeTestableDbContext(DbContextOptions<NoSafeTestableDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestableEntity>().ToTable("TestableEntity");
        modelBuilder.Entity<TestableEntity>().HasKey(t => t.Id);
        modelBuilder.Entity<TestableEntity>().HasMany<TestableEntityDependency>(t => t.Dependencies).WithOne()
            .HasForeignKey(t => t.TestableEntityId);
        modelBuilder.Entity<TestableEntity>().Property(t => t.Name);


        modelBuilder.Entity<TestableEntityDependency>().ToTable("TestableEntityDependency");
        modelBuilder.Entity<TestableEntityDependency>().HasKey(t => t.Id);
        modelBuilder.Entity<TestableEntityDependency>().Property(t => t.Name);

        base.OnModelCreating(modelBuilder);
    }
}