using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
[Collection("Sequential")]
public class InMemoryTests
{
    private const int IdOffset = 1000;
    private const int NbTestableEntityAlreadyInDb = 3;

    private static readonly TestableEntity[] EntitiesInDb =
    {
        new()
        {
            Id = 1,
            Name = "Name1",
            Dependencies = new List<TestableEntityDependency>
            {
                new()
                {
                    Id = 1,
                    Name = "Name1"
                }
            }
        },
        new()
        {
            Id = 2,
            Name = "Name2"
        },
        new()
        {
            Id = 3,
            Name = "Name2"
        }
    };

    private readonly DbContext _testableDbContext = CreateFromConnection();

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            db.Set<TestableEntity>().Add(new TestableEntity
            {
                Id = i,
                Name = i.ToString()
            });
            db.SaveChanges();
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddAsyncShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddAsync(new TestableEntity
            {
                Id = i,
                Name = i.ToString()
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            db.Set<TestableEntity>().AddRange(new TestableEntity
            {
                Id = i,
                Name = i.ToString()
            });
            db.SaveChanges();
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeAsyncShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddRangeAsync(new TestableEntity
            {
                Id = i,
                Name = i.ToString()
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AttachShouldAttachAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().Attach(new TestableEntity
            {
                Id = i,
                Name = i.ToString()
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task WhereShouldApplyFilter(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().Where(t => t.Name == "Name2").ToList();

            testableEntities.Should().HaveCount(2);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderByShouldApplyOrder(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().OrderBy(t => t.Name).ToList();
            var entities = EntitiesInDb.OrderBy(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderDescendingByShouldApplyOrder(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().OrderByDescending(t => t.Name).ToList();
            var entities = EntitiesInDb.OrderByDescending(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectShouldApplySelect(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().Select(t => t.Name).ToList();
            var entities = EntitiesInDb.Select(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Should().Be(entities[index]);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToListAsyncShouldApplyToListAsync(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().ToListAsync();
            var entities = EntitiesInDb.ToList();
            testableEntities.Should().BeEquivalentTo(entities);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToArrayAsyncShouldApplyToArrayAsync(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().ToArrayAsync();
            testableEntities.Should().BeEquivalentTo(EntitiesInDb);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ComplexQueryShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().Where(t => t.Name == "Name2")
                .OrderByDescending(t => t.Id).Select(t => t.Id).ToListAsync();
            var entities = EntitiesInDb.Where(t => t.Name == "Name2").OrderByDescending(t => t.Id)
                .Select(t => t.Id).ToList();
            testableEntities.Should().BeEquivalentTo(entities);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateShouldUpdateAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Set<TestableEntity>().Find(1);
            testableEntity!.Name = "NewName";
            db.Set<TestableEntity>().Update(testableEntity);
            db.SaveChanges();
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.Id == 1).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateRangeShouldUpdateAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.Set<TestableEntity>().FindAsync(1);
            testableEntity!.Name = "NewName";
            db.Set<TestableEntity>().UpdateRange(testableEntity);
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.Id == 1).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task IncludeDependenciesShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity =
                await db.Set<TestableEntity>().Include(t => t.Dependencies).FirstOrDefaultAsync();
            testableEntity.Should().NotBeNull();
            testableEntity!.Dependencies.Should().HaveCount(EntitiesInDb.First().Dependencies.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindGenericShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Find<TestableEntity>(1);
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(1);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Find(typeof(TestableEntity), 1) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(1);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindAsyncGenericShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync<TestableEntity>(1);
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(1);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindAsyncShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync(typeof(TestableEntity), 1) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(1);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    private static Task[] CreateTasks(int nbTasks, DbContext dbContext, Func<int, DbContext, Task> action)
    {
        return Enumerable.Range(1, nbTasks)
            .Select(i => Task.Run(() => action(i + IdOffset, dbContext)))
            .ToArray();
    }

    private static DbContext CreateFromConnection()
    {
        DbContext dbContext = new TestableDbContext(new DbContextOptionsBuilder<TestableDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        CleanExistingData(dbContext);
        FillTestData(dbContext);
        return dbContext;
    }

    private static void FillTestData(DbContext dbContext)
    {
        dbContext.Set<TestableEntity>().AddRange(EntitiesInDb);
        dbContext.SaveChanges();
    }

    private static void CleanExistingData(DbContext dbContext)
    {
        var testableEntities = dbContext.Set<TestableEntity>().ToList();
        dbContext.Set<TestableEntity>().RemoveRange(testableEntities);
        dbContext.SaveChanges();
    }
}