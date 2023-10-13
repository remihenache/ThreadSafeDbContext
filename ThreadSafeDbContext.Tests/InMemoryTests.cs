using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
[Collection("Sequential")]
public class InMemoryTests
{
    private const Int32 IdOffset = 1000;
    private const Int32 NbTestableEntityAlreadyInDb = 3;

    private static readonly TestableEntity[] EntitiesInDb =
    {
        new()
        {
            ID = 1,
            Name = "Name1",
            Dependencies = new List<TestableEntityDependency>
            {
                new()
                {
                    ID = 1,
                    Name = "Name1"
                }
            }
        },
        new()
        {
            ID = 2,
            Name = "Name2"
        },
        new()
        {
            ID = 3,
            Name = "Name2"
        }
    };

    private readonly DbContext testableDbContext = CreateFromConnection();

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddShouldAddAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().Add(new TestableEntity
            {
                ID = i
            });
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddAsyncShouldAddAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddAsync(new TestableEntity
            {
                ID = i
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeShouldAddAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().AddRange(new TestableEntity
            {
                ID = i
            });
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeAsyncShouldAddAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddRangeAsync(new TestableEntity
            {
                ID = i
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AttachShouldAttachAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().Attach(new TestableEntity
            {
                ID = i
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task WhereShouldApplyFilter(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<TestableEntity> testableEntities = db.Set<TestableEntity>().Where(t => t.Name == "Name2").ToList();

            testableEntities.Should().HaveCount(2);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderByShouldApplyOrder(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<TestableEntity> testableEntities = db.Set<TestableEntity>().OrderBy(t => t.Name).ToList();
            List<TestableEntity> entities = EntitiesInDb.OrderBy(t => t.Name).ToList();
            for (Int32 index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderDescendingByShouldApplyOrder(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<TestableEntity> testableEntities = db.Set<TestableEntity>().OrderByDescending(t => t.Name).ToList();
            List<TestableEntity> entities = EntitiesInDb.OrderByDescending(t => t.Name).ToList();
            for (Int32 index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectShouldApplySelect(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<String> testableEntities = db.Set<TestableEntity>().Select(t => t.Name).ToList();
            List<String> entities = EntitiesInDb.Select(t => t.Name).ToList();
            for (Int32 index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Should().Be(entities[index]);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToListAsyncShouldApplyToListAsync(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<TestableEntity> testableEntities = await db.Set<TestableEntity>().ToListAsync();
            List<TestableEntity> entities = EntitiesInDb.ToList();
            testableEntities.Should().BeEquivalentTo(entities);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToArrayAsyncShouldApplyToArrayAsync(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            TestableEntity[] testableEntities = await db.Set<TestableEntity>().ToArrayAsync();
            testableEntities.Should().BeEquivalentTo(EntitiesInDb);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ComplexQueryShouldWork(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            List<Int32> testableEntities = await db.Set<TestableEntity>().Where(t => t.Name == "Name2")
                .OrderByDescending(t => t.ID).Select(t => t.ID).ToListAsync();
            List<Int32> entities = EntitiesInDb.Where(t => t.Name == "Name2").OrderByDescending(t => t.ID)
                .Select(t => t.ID).ToList();
            testableEntities.Should().BeEquivalentTo(entities);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateShouldUpdateAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            TestableEntity? testableEntity = db.Set<TestableEntity>().Find(1);
            testableEntity.Name = "NewName";
            db.Set<TestableEntity>().Update(testableEntity);
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.ID == 1).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateRangeShouldUpdateAnEntity(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            TestableEntity? testableEntity = await db.Set<TestableEntity>().FindAsync(1);
            testableEntity.Name = "NewName";
            db.Set<TestableEntity>().UpdateRange(testableEntity);
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        List<TestableEntity> entities = await this.testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.ID == 1).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task IncludeDependenciesShouldWork(Int32 nbTasks)
    {
        Task[] tasks = CreateTasks(nbTasks, this.testableDbContext, async (i, db) =>
        {
            TestableEntity? testableEntity =
                await db.Set<TestableEntity>().Include(t => t.Dependencies).FirstOrDefaultAsync();
            testableEntity.Should().NotBeNull();
            testableEntity!.Dependencies.Should().HaveCount(EntitiesInDb.First().Dependencies.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    private static Task[] CreateTasks(Int32 nbTasks, DbContext dbContext, Func<Int32, DbContext, Task> action)
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
        List<TestableEntity> testableEntities = dbContext.Set<TestableEntity>().ToList();
        dbContext.Set<TestableEntity>().RemoveRange(testableEntities);
        dbContext.SaveChanges();
    }
}