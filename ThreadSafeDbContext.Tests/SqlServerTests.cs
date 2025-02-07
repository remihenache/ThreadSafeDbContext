using FluentAssertions;
using Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests;

[TestCaseOrderer("ThreadSafeDbContext.Tests.AlphabeticalOrderer", "ThreadSafeDbContext.Tests")]
[Collection("Sequential")]
public class SqlServerTests
{
    private const int IdOffset = 1000;
    private const int NbTestableEntityAlreadyInDb = 3;

    private readonly DbContext _testableDbContext;

    public SqlServerTests()
    {
        _testableDbContext = CreateFromConnection();
    }


    private TestableEntity[] GetTestableEntities()
    {
        return GetAllTestableEntities().Where(t => !string.IsNullOrEmpty(t.Name)).ToArray();
    }

    private TestableEntity[] GetAllTestableEntities()
    {
        TestableEntity[] entitiesInDb =
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
            },
            new()
            {
                Name = ""
            }
        };
        return entitiesInDb;
    }


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
                Name = "TestInsert"
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
                Name = "TestInsert"
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
                Name = "TestInsert"
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
                Name = "TestInsert"
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
                Name = "TestInsert"
            });
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb + nbTasks);
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
            var entities = GetTestableEntities().OrderBy(t => t.Name).ToList();
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
            var entities = GetTestableEntities().OrderByDescending(t => t.Name).ToList();
            for (var index = 0; index < testableEntities.Count; index++)
                testableEntities[index].Name.Should().Be(entities[index].Name);
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectShouldApplySelect(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntities = db.Set<TestableEntity>().Select(t => t.Name).ToList();
            var entities = GetTestableEntities().Select(t => t.Name).ToList();
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
            var entities = GetTestableEntities().ToList();
            testableEntities.Should().HaveCount(entities.Count);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task ToArrayAsyncShouldApplyToArrayAsync(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
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
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntities = await db.Set<TestableEntity>().Where(t => t.Name == "Name2")
                .OrderByDescending(t => t.Id).Select(t => t.Id).ToListAsync();
            var entities = GetTestableEntities().Where(t => t.Name == "Name2").OrderByDescending(t => t.Id)
                .Select(t => t.Id).ToList();
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
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Set<TestableEntity>().Find(idToFind);
            testableEntity!.Name = "NewName";
            db.Set<TestableEntity>().Update(testableEntity);
            db.SaveChanges();
            return Task.CompletedTask;
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.Id == idToFind).Name.Should().Be("NewName");
    }


    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task UpdateRangeShouldUpdateAnEntity(int nbTasks)
    {
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.Set<TestableEntity>().FindAsync(idToFind);
            testableEntity!.Name = "NewName";
            db.Set<TestableEntity>().UpdateRange(testableEntity);
            await db.SaveChangesAsync();
        }).ToArray();

        await Task.WhenAll(tasks);

        var entities = await _testableDbContext.Set<TestableEntity>().ToListAsync();
        entities.Should().HaveCount(NbTestableEntityAlreadyInDb);
        entities.First(t => t.Id == idToFind).Name.Should().Be("NewName");
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task SelectDependenciesShouldWork(int nbTasks)
    {
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
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
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
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
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Find<TestableEntity>(idToFind);
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(idToFind);
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
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, (i, db) =>
        {
            var testableEntity = db.Find(typeof(TestableEntity), idToFind) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(idToFind);
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
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync<TestableEntity>(idToFind);
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task FindAsyncShouldWork(int nbTasks)
    {
        var idToFind = _testableDbContext.Set<TestableEntity>().First().Id;
        var tasks = CreateTasks(nbTasks, _testableDbContext, async (i, db) =>
        {
            var testableEntity = await db.FindAsync(typeof(TestableEntity), idToFind) as TestableEntity;
            testableEntity.Should().NotBeNull();
            testableEntity!.Id.Should().Be(idToFind);
        }).ToArray();

        await Task.WhenAll(tasks);
    }


    [Fact]
    public async Task ThreadSafeDbContext_Handles_IgnoreQueryFilters_Correctly()
    {
        var query = _testableDbContext.Set<TestableEntity>()
            .IgnoreQueryFilters();
        var count = await query.CountAsync();
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task ThreadSafeDbContext_FiltersResultsBy_RLSFlag()
    {
        var query = _testableDbContext.Set<TestableEntity>();
        var count = await query.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task ThreadSafeDbContext_SetMethod_Handles_IgnoreQueryFilters_Correctly()
    {
        var query = _testableDbContext.Set<TestableEntity>()
            .IgnoreQueryFilters();
        var count = await query.CountAsync();

        // The expected result is 2, but the actual result is 1.
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task ThreadSafeDbContext_SetMethod_FiltersResultsBy_RLSFlag()
    {
        var query = _testableDbContext.Set<TestableEntity>();
        var count = await query.CountAsync();
        Assert.Equal(3, count);
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
        dbContext.Set<TestableEntity>().AddRange(GetAllTestableEntities());
        dbContext.SaveChanges();
    }

    private static void CleanExistingData(DbContext dbContext)
    {
        var testableEntityDependency = dbContext.Set<TestableEntityDependency>().ToList();
        dbContext.Set<TestableEntityDependency>().RemoveRange(testableEntityDependency);
        var testableEntities = dbContext.Set<TestableEntity>().IgnoreQueryFilters().ToList();
        dbContext.Set<TestableEntity>().RemoveRange(testableEntities);
        dbContext.SaveChanges();
    }
}