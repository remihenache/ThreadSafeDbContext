using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
public class MemoryTests
{
    [Fact]
    public async Task ShouldCreateAnEntity()
    {
        TestableDbContext testableDbContext = CreateFromConnection("Test");
        testableDbContext.Set<TestableEntity>().Add(new TestableEntity
        {
            ID = 1
        });
        await testableDbContext.SaveChangesAsync();
        TestableEntity? entity = await testableDbContext.Set<TestableEntity>().FindAsync(1);
        entity.Should().NotBeNull();
        entity!.ID.Should().Be(1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ShouldCreateAnEntityFromMultipleThreads(Int32 nbTasks)
    {
        TestableDbContext testableDbContext = CreateFromConnection($"Test{nbTasks}");
        Task[] tasks = CreateTasks(nbTasks, testableDbContext).ToArray();
        tasks.Length.Should().Be(nbTasks);
        await Task.WhenAll(tasks);
        Int32 count = testableDbContext.Set<TestableEntity>().Count();
        count.Should().Be(nbTasks);
        List<TestableEntity> entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(100)]
    public async Task ShouldReadAnEntityFromMultipleThreads(Int32 nbTasks)
    {
        TestableDbContext testableDbContext = CreateFromConnection($"TestRead{nbTasks}");
        testableDbContext.Set<TestableEntity>().Add(new TestableEntity {ID = 1});
        await testableDbContext.SaveChangesAsync();

        IEnumerable<Task> tasks = CreateReadTasks(nbTasks, testableDbContext);
        await Task.WhenAll(tasks.ToArray());
        Assert.True(true);
    }

    private static IEnumerable<Task> CreateReadTasks(Int32 numberOfOccurrences, DbContext threadSafeDbContext)
    {
        List<Task> result = new();
        for (Int32 index = 0; index < numberOfOccurrences; index++)
            result.Add(ReadEntity(threadSafeDbContext));
        return result;
    }

    private static Task ReadEntity(DbContext threadSafeDbContext)
    {
        return Task.Run(() => threadSafeDbContext.Set<TestableEntity>().FirstAsync());
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
            dbContext.Set<TestableEntity>().Add(new TestableEntity
            {
                ID = key
            });
            await dbContext.SaveChangesAsync();
        });
    }

    private static TestableDbContext CreateFromConnection(String connection)
    {
        return new TestableDbContext(new DbContextOptionsBuilder<TestableDbContext>()
            .UseInMemoryDatabase(connection));
    }
}