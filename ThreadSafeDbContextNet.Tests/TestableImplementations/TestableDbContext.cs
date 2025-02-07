using System.Data.Entity;

namespace ThreadSafeDbContextNet.Tests.TestableImplementations
{
    public class TestableDbContext : ThreadSafeDbContext
    {
        public TestableDbContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestableEntity>().ToTable("TestableEntity");
            modelBuilder.Entity<TestableEntity>().HasKey(t => t.Id);
            modelBuilder.Entity<TestableEntity>().HasMany(t => t.Dependencies).WithRequired(d => d.TestableEntity)
                .HasForeignKey(t => t.TestableEntityId);
            modelBuilder.Entity<TestableEntity>().Property(t => t.Name);


            modelBuilder.Entity<TestableEntityDependency>().ToTable("TestableEntityDependency");
            modelBuilder.Entity<TestableEntityDependency>().HasKey(t => t.Id);
            modelBuilder.Entity<TestableEntityDependency>().Property(t => t.Name);

            base.OnModelCreating(modelBuilder);
        }
    }
}