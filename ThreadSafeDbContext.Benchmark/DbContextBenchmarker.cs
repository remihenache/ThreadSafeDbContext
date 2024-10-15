using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;

namespace ThreadSafeDbContext.Benchmark;

public class DbContextBenchmarker
{
    private static readonly BenchMarkEntity[] EntitiesInDb =
    {
        new()
        {
            Id = 1,
            Name = "Name1"
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
        },
        new()
        {
            Id = 4,
            Name = "Name2"
        },
        new()
        {
            Id = 5,
            Name = "Name2"
        },
        new()
        {
            Id = 6,
            Name = "Name2"
        },
        new()
        {
            Id = 7,
            Name = "Name3"
        },
        new()
        {
            Id = 8,
            Name = "Name3"
        },
        new()
        {
            Id = 9,
            Name = "Name3"
        },
        new()
        {
            Id = 10,
            Name = "Name3"
        },
        new()
        {
            Id = 11,
            Name = "Name4"
        },
        new()
        {
            Id = 12,
            Name = "Name4"
        },
        new()
        {
            Id = 13,
            Name = "Name4"
        },
        new()
        {
            Id = 14,
            Name = "Name5"
        },
        new()
        {
            Id = 15,
            Name = "Name6"
        },
        new()
        {
            Id = 16,
            Name = "Name7"
        },
        new()
        {
            Id = 17,
            Name = "Name8"
        },
        new()
        {
            Id = 18,
            Name = "Name9"
        },
        new()
        {
            Id = 19,
            Name = "Name10"
        },
        new()
        {
            Id = 20,
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

        var benchMarkEntitiesName2 =
            await dbContext.Set<BenchMarkEntity>().Where(e => e.Name == "Name2").ToListAsync();
        var benchMarkEntitiesName3 = await dbContext.Set<BenchMarkEntity>()
            .Where(e => e.Name == "Name3").OrderByDescending(e => e.Id).ToListAsync();
        var benchMarkEntitiesNames = await dbContext.Set<BenchMarkEntity>().Select(e => e.Name).ToListAsync();

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

        var benchMarkEntitiesName2 =
            await dbContext.Set<BenchMarkEntity>().Where(e => e.Name == "Name2").ToListAsync();
        var benchMarkEntitiesName3 = await dbContext.Set<BenchMarkEntity>()
            .Where(e => e.Name == "Name3").OrderByDescending(e => e.Id).ToListAsync();
        var benchMarkEntitiesNames = await dbContext.Set<BenchMarkEntity>().Select(e => e.Name).ToListAsync();

        dbContext.Set<BenchMarkEntity>().RemoveRange(EntitiesInDb);
        await dbContext.SaveChangesAsync();
    }
}