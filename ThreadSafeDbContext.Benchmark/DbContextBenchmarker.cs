using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class DbContextBenchmarker
{
    private static readonly BenchMarkEntity[] EntitiesInDb =
    {
        new()
        {
            ID = 1,
            Name = "Name1"
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
        },
        new()
        {
            ID = 4,
            Name = "Name2"
        },
        new()
        {
            ID = 5,
            Name = "Name2"
        },
        new()
        {
            ID = 6,
            Name = "Name2"
        },
        new()
        {
            ID = 7,
            Name = "Name3"
        },
        new()
        {
            ID = 8,
            Name = "Name3"
        },
        new()
        {
            ID = 9,
            Name = "Name3"
        },
        new()
        {
            ID = 10,
            Name = "Name3"
        },
        new()
        {
            ID = 11,
            Name = "Name4"
        },
        new()
        {
            ID = 12,
            Name = "Name4"
        },
        new()
        {
            ID = 13,
            Name = "Name4"
        },
        new()
        {
            ID = 14,
            Name = "Name5"
        },
        new()
        {
            ID = 15,
            Name = "Name6"
        },
        new()
        {
            ID = 16,
            Name = "Name7"
        },
        new()
        {
            ID = 17,
            Name = "Name8"
        },
        new()
        {
            ID = 18,
            Name = "Name9"
        },
        new()
        {
            ID = 19,
            Name = "Name10"
        },
        new()
        {
            ID = 20,
            Name = "Name11"
        }
    };

    [Benchmark]
    public async Task StandardDbContext()
    {
        DbContext dbContext =
            new BenchMarkDbContext(
                new DbContextOptionsBuilder<BenchMarkDbContext>().UseInMemoryDatabase("test").Options);
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
                new DbContextOptionsBuilder<BenchMarkThreadSafeDbContext>().UseInMemoryDatabase("test1").Options);
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