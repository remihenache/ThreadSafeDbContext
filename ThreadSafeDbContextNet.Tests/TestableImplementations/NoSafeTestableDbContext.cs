using System.Data.Entity;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations
{
    public class NoSafeTestableDbContext : DbContext
    {

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestableEntity>().ToTable("TestableEntity");
            modelBuilder.Entity<TestableEntity>().HasKey(t => t.Id);
            modelBuilder.Entity<TestableEntity>().HasMany<TestableEntityDependency>(t => t.Dependencies).WithRequired()
                .HasForeignKey(t => t.TestableEntityId);
            modelBuilder.Entity<TestableEntity>().Property(t => t.Name);


            modelBuilder.Entity<TestableEntityDependency>().ToTable("TestableEntityDependency");
            modelBuilder.Entity<TestableEntityDependency>().HasKey(t => t.Id);
            modelBuilder.Entity<TestableEntityDependency>().Property(t => t.Name);

            base.OnModelCreating(modelBuilder);
        }
    }
}