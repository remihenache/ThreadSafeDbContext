using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
[Collection("Sequential")]
public class SqlServerTests
{
    private const int IdOffset = 1000;
    private const int NbTestableEntityAlreadyInDb = 3;

    private readonly DbContext testableDbContext;

    public SqlServerTests()
    {
        testableDbContext = CreateFromConnection();
    }

    private TestableEntity[] GetTestableEntities()
    {
        TestableEntity[] EntitiesInDb =
        {
            new()
            {
                Name = "Name1",
                Dependencies = new List<TestableEntityDependency>
                {
                    new()
                    {
                        Name = "Name1"
                    }
                }
            },
            new()
            {
                Name = "Name2"
            },
            new()
            {
                Name = "Name2"
            }
        };
        return EntitiesInDb;
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().Add(new TestableEntity
            {
                Name = "TestInsert"
            });
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddAsyncShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddAsync(new TestableEntity
            {
                Name = "TestInsert"
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().AddRange(new TestableEntity
            {
                Name = "TestInsert"
            });
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AddRangeAsyncShouldAddAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            await db.Set<TestableEntity>().AddRangeAsync(new TestableEntity
            {
                Name = "TestInsert"
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(nbTasks + NbTestableEntityAlreadyInDb);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task AttachShouldAttachAnEntity(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            db.Set<TestableEntity>().Attach(new TestableEntity
            {
                Name = "TestInsert"
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb + nbTasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task WhereShouldApplyFilter(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().Where(t => t.Name == "Name2").ToList();

            testableEntities.Should().HaveCount(2);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderByShouldApplyOrder(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().OrderBy(t => t.Name).ToList();
            var entities = GetTestableEntities().OrderBy(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task OrderDescendingByShouldApplyOrder(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().OrderByDescending(t => t.Name).ToList();
            var entities = GetTestableEntities().OrderByDescending(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectShouldApplySelect(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().Select(t => t.Name).ToList();
            var entities = GetTestableEntities().Select(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Should().Be(entities[index]);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToListAsyncShouldApplyToListAsync(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().ToListAsync();
            var entities = GetTestableEntities().ToList();
            testableEntities.Should().HaveCount(entities.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToArrayAsyncShouldApplyToArrayAsync(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().ToArrayAsync();
            testableEntities.Should().HaveCount(GetTestableEntities().Length);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ComplexQueryShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().Where(t => t.Name == "Name2")
                .OrderByDescending(t => t.ID).Select(t => t.ID).ToListAsync();
            var entities = GetTestableEntities().Where(t => t.Name == "Name2").OrderByDescending(t => t.ID)
                .Select(t => t.ID).ToList();
            testableEntities.Should().HaveCount(entities.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateShouldUpdateAnEntity(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = db.Set<TestableEntity>().Find(idToFind);
            testableEntity.Name = "NewName";
            db.Set<TestableEntity>().Update(testableEntity);
            db.SaveChanges();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.ID == idToFind).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateRangeShouldUpdateAnEntity(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.Set<TestableEntity>().FindAsync(idToFind);
            testableEntity.Name = "NewName";
            db.Set<TestableEntity>().UpdateRange(testableEntity);
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.ID == idToFind).Name.Should().Be("NewName");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectDependenciesShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.Set<TestableEntity>().Where(t => t.Name == "Name1")
                .SelectMany(t => t.Dependencies).FirstOrDefaultAsync();
            testableEntity.Should().NotBeNull();
            testableEntity!.Name.Should().Be("Name1");
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task IncludeDependenciesShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity =
                await db.Set<TestableEntity>().Include(t => t.Dependencies).FirstOrDefaultAsync();
            testableEntity.Should().NotBeNull();
            testableEntity!.Dependencies.Should().HaveCount(GetTestableEntities().First().Dependencies.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindGenericShouldWork(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = db.Find<TestableEntity>(idToFind);
            testableEntity.Should().NotBeNull();
            testableEntity.ID.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindShouldWork(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = db.Find(typeof(TestableEntity), idToFind) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity.ID.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindAsyncGenericShouldWork(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync<TestableEntity>(idToFind);
            testableEntity.Should().NotBeNull();
            testableEntity.ID.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindAsyncShouldWork(int nbTasks)
    {
        var idToFind = testableDbContext.Set<TestableEntity>().First().ID;
        var tasks = CreateTasks(nbTasks, testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync(typeof(TestableEntity), idToFind) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity.ID.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    private static Task[] CreateTasks(int nbTasks, DbContext dbContext, Func<int, DbContext, Task> action)
    {
        return Enumerable.Range(1, nbTasks)
            .Select(i => Task.Run(() => action(i + IdOffset, dbContext)))
            .ToArray();
    }

    private DbContext CreateFromConnection()
    {
        DbContext dbContext = new TestableDbContext(new DbContextOptionsBuilder<TestableDbContext>()
            .UseSqlServer(
                "Server=localhost,1436;Database=ThreadSafeDbContext;TrustServerCertificate=true;MultipleActiveResultSets=true;User ID=sa;Password=P@ssword11!!;",
                options => options.EnableRetryOnFailure()
            )
            .Options);
        dbContext.Database.EnsureCreated();

        CleanExistingData(dbContext);
        FillTestData(dbContext);
        return dbContext;
    }

    private void FillTestData(DbContext dbContext)
    {
        dbContext.Set<TestableEntity>().AddRange(GetTestableEntities());
        dbContext.SaveChanges();
    }

    private static void CleanExistingData(DbContext dbContext)
    {
        var testableEntityDependency = dbContext.Set<TestableEntityDependency>().ToList();
        dbContext.Set<TestableEntityDependency>().RemoveRange(testableEntityDependency);
        var testableEntities = dbContext.Set<TestableEntity>().ToList();
        dbContext.Set<TestableEntity>().RemoveRange(testableEntities);
        dbContext.SaveChanges();
    }
}