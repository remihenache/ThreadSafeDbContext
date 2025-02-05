using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkNet.ThreadSafe;

public class ThreadSafeDbContext : DbContext
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    
    public ThreadSafeDbContext()
    {
    }

    public ThreadSafeDbContext(DbContextOptions options)
        : base(new DbContextOptionsBuilder(options)
            .EnableThreadSafetyChecks(false)
            .Options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableThreadSafetyChecks(false);
        base.OnConfiguring(optionsBuilder);
    }

    public override DbSet<TEntity> Set<TEntity>()
    {
        _semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeDbSet<TEntity>(this, typeof(TEntity).FullName!, _semaphoreSlim);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }


    public override async Task<int> SaveChangesAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            return await base.SaveChangesAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override int SaveChanges()
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.SaveChanges();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public new void Dispose()
    {
        _semaphoreSlim.Dispose();
        base.Dispose();
    }

}