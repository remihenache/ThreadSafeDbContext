using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class DbContextBenchmarker
{
    private static readonly BenchMarkEntity[] EntitiesInDb =
    {
        new BenchMarkEntity
        {
            ID = 1,
            Name = "Name1"
        },
        new BenchMarkEntity
        {
            ID = 2,
            Name = "Name2"
        },
        new BenchMarkEntity
        {
            ID = 3,
            Name = "Name2"
        },
        new BenchMarkEntity
        {
            ID = 4,
            Name = "Name2"
        },
        new BenchMarkEntity
        {
            ID = 5,
            Name = "Name2"
        },
        new BenchMarkEntity
        {
            ID = 6,
            Name = "Name2"
        },
        new BenchMarkEntity
        {
            ID = 7,
            Name = "Name3"
        },
        new BenchMarkEntity
        {
            ID = 8,
            Name = "Name3"
        },
        new BenchMarkEntity
        {
            ID = 9,
            Name = "Name3"
        },
        new BenchMarkEntity
        {
            ID = 10,
            Name = "Name3"
        },
        new BenchMarkEntity
        {
            ID = 11,
            Name = "Name4"
        },
        new BenchMarkEntity
        {
            ID = 12,
            Name = "Name4"
        },
        new BenchMarkEntity
        {
            ID = 13,
            Name = "Name4"
        },
        new BenchMarkEntity
        {
            ID = 14,
            Name = "Name5"
        },
        new BenchMarkEntity
        {
            ID = 15,
            Name = "Name6"
        },
        new BenchMarkEntity
        {
            ID = 16,
            Name = "Name7"
        },
        new BenchMarkEntity
        {
            ID = 17,
            Name = "Name8"
        },
        new BenchMarkEntity
        {
            ID = 18,
            Name = "Name9"
        },
        new BenchMarkEntity
        {
            ID = 19,
            Name = "Name10"
        },
        new BenchMarkEntity
        {
            ID = 20,
            Name = "Name11"
        }
    };

    [Benchmark]
    public async Task StandardDbContext()
    {
        DbContext dbContext =
            new BenchMarkDbContext(new DbContextOptionsBuilder<BenchMarkDbContext>().UseInMemoryDatabase("test"));
        await dbContext.Set<BenchMarkEntity>().AddRangeAsync(EntitiesInDb);
        await dbContext.SaveChangesAsync();

        List<BenchMarkEntity> benchMarkEntitiesName2 =
            await dbContext.Set<BenchMarkEntity>().Where(e => e.Name == "Name2").ToListAsync();
        List<BenchMarkEntity> benchMarkEntitiesName3 = await dbContext.Set<BenchMarkEntity>()
            .Where(e => e.Name == "Name3").OrderByDescending(e => e.ID).ToListAsync();
        List<String> benchMarkEntitiesNames = await dbContext.Set<BenchMarkEntity>().Select(e => e.Name).ToListAsync();

        dbContext.Set<BenchMarkEntity>().RemoveRange(EntitiesInDb);
        await dbContext.SaveChangesAsync();
    }

    [Benchmark]
    public async Task ThreadSafeDbContext()
    {
        DbContext dbContext =
            new BenchMarkThreadSafeDbContext(
                new DbContextOptionsBuilder<BenchMarkThreadSafeDbContext>().UseInMemoryDatabase("test1"));
        await dbContext.Set<BenchMarkEntity>().AddRangeAsync(EntitiesInDb);
        await dbContext.SaveChangesAsync();

        List<BenchMarkEntity> benchMarkEntitiesName2 =
            await dbContext.Set<BenchMarkEntity>().Where(e => e.Name == "Name2").ToListAsync();
        List<BenchMarkEntity> benchMarkEntitiesName3 = await dbContext.Set<BenchMarkEntity>()
            .Where(e => e.Name == "Name3").OrderByDescending(e => e.ID).ToListAsync();
        List<String> benchMarkEntitiesNames = await dbContext.Set<BenchMarkEntity>().Select(e => e.Name).ToListAsync();

        dbContext.Set<BenchMarkEntity>().RemoveRange(EntitiesInDb);
        await dbContext.SaveChangesAsync();
    }
}