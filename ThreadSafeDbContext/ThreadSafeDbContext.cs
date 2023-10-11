using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ThreadSafeDbContext;

public class ThreadSafeDbContext : DbContext
{
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    public ThreadSafeDbContext()
    {
    }

    public ThreadSafeDbContext(DbContextOptionsBuilder<ThreadSafeDbContext> optionsBuilder)
        : base(optionsBuilder
            .ReplaceService<IConcurrencyDetector, ThreadSafeConcurrencyDetector>()
            .Options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ReplaceService<IConcurrencyDetector, ThreadSafeConcurrencyDetector>();
        base.OnConfiguring(optionsBuilder);
    }

    public override DbSet<TEntity> Set<TEntity>()
    {
        this.semaphoreSlim.Wait();
        ThreadSafeDbSet<TEntity> result = new(this, base.Set<TEntity>(), this.semaphoreSlim);
        this.semaphoreSlim.Release();
        return result;
    }


    public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        try
        {
            await this.semaphoreSlim.WaitAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }

    public override Int32 SaveChanges()
    {
        try
        {
            this.semaphoreSlim.Wait();
            return base.SaveChanges();
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}