using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
[Collection("Sequential")]
public class IntegrationTests : IDisposable
{
    private readonly TestableDbContext testableDbContext;

    public IntegrationTests()
    {
        this.testableDbContext = CreateFromConnection();
    }

    public void Dispose()
    {
        List<TestableEntity> testableEntities = this.testableDbContext.Set<TestableEntity>().ToList();
        this.testableDbContext.Set<TestableEntity>().RemoveRange(testableEntities);
        this.testableDbContext.SaveChanges();
    }

    [Fact]
    public async Task ShouldCreateAnEntity()
    {
        this.testableDbContext.Set<TestableEntity>().Add(new TestableEntity
        {
            ID = 1
        });
        await this.testableDbContext.SaveChangesAsync();
        TestableEntity? entity = await this.testableDbContext.Set<TestableEntity>().FindAsync(1);
        entity.Should().NotBeNull();
        entity!.ID.Should().Be(1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ShouldCreateAnEntityFromMultipleThreads(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext).ToArray();
        tasks.Length.Should().Be(nbTasks);
        await Task.WhenAll(tasks);
        Int32 count = this.testableDbContext.Set<TestableEntity>().Count();
        count.Should().Be(nbTasks);
        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ShouldUpdateAnEntityFromMultipleThreads(Int32 nbTasks)
    {
        this.testableDbContext.Set<TestableEntity>().Add(new TestableEntity
        {
            ID = 1
        });
        await this.testableDbContext.SaveChangesAsync();
        Task[] tasks = UpdateTasks(nbTasks, this.testableDbContext).ToArray();
        tasks.Length.Should().Be(nbTasks);
        await Task.WhenAll(tasks);
        Int32 count = this.testableDbContext.Set<TestableEntity>().Count();
        count.Should().Be(1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(100)]
    public async Task ShouldReadAnEntityFromMultipleThreads(Int32 nbTasks)
    {
        this.testableDbContext.Set<TestableEntity>().Add(new TestableEntity {ID = 1});
        await this.testableDbContext.SaveChangesAsync();

        IEnumerable<Task> tasks = CreateReadTasks(nbTasks, this.testableDbContext);
        await Task.WhenAll(tasks.ToArray());
        Assert.True(true);
    }

    private static IEnumerable<Task> CreateReadTasks(Int32 numberOfOccurrences, DbContext threadSafeDbContext)
    {
        List<Task> result = new();
        for (Int32 index = 0; index < numberOfOccurrences; index++)
        {
            result.Add(ReadOneEntity(threadSafeDbContext));
            result.Add(ReadAllEntities(threadSafeDbContext));
            result.Add(FindOneEntity(threadSafeDbContext));
        }

        return result;
    }

    private static Task ReadOneEntity(DbContext threadSafeDbContext)
    {
        return Task.Run(() => threadSafeDbContext.Set<TestableEntity>().FirstAsync());
    }

    private static Task FindOneEntity(DbContext threadSafeDbContext)
    {
        return Task.Run(() => threadSafeDbContext.Set<TestableEntity>().FindAsync(1));
    }


    private static Task ReadAllEntities(DbContext threadSafeDbContext)
    {
        return Task.Run(() => threadSafeDbContext.Set<TestableEntity>().ToListAsync());
    }

    private static IEnumerable<Task> CreateTasks(Int32 numberOfOccurrences, DbContext threadSafeDbContext)
    {
        List<Task> result = new();
        for (Int32 index = 0; index < numberOfOccurrences; index++)
            result.Add(CreateEntity(index + numberOfOccurrences, threadSafeDbContext));
        return result;
    }

    private static Task CreateEntity(Int32 key, DbContext dbContext)
    {
        return Task.Run(async () =>
        {
            await dbContext.Set<TestableEntity>().AddAsync(new TestableEntity
            {
                ID = key
            });
            await dbContext.SaveChangesAsync();
        });
    }

    private static IEnumerable<Task> UpdateTasks(Int32 numberOfOccurrences, DbContext threadSafeDbContext)
    {
        List<Task> result = new();
        for (Int32 index = 0; index < numberOfOccurrences; index++)
            result.Add(UpdateEntity(index + numberOfOccurrences, threadSafeDbContext));
        return result;
    }

    private static Task UpdateEntity(Int32 key, DbContext dbContext)
    {
        return Task.Run(async () =>
        {
            TestableEntity? testableEntity = dbContext.Find<TestableEntity>(1);
            dbContext.Set<TestableEntity>().Update(testableEntity!);
            await dbContext.SaveChangesAsync();
        });
    }

    private static TestableDbContext CreateFromConnection()
    {
        return new TestableDbContext(new DbContextOptionsBuilder<TestableDbContext>()
            .UseSqlServer(
                "Server=localhost,1436;Database=ThreadSafeDbContext;TrustServerCertificate=true;MultipleActiveResultSets=true;User ID=sa;Password=P@ssword11!!;", //MyP@ssw0rd!
                options => options.EnableRetryOnFailure()
            ));
    }
}