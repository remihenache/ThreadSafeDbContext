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
        try
        {
            this.semaphoreSlim.Wait();
            return new ThreadSafeDbSet<TEntity>(base.Set<TEntity>(), this.semaphoreSlim);
        }
        finally
        {
            if(this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
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
            if(this.semaphoreSlim.CurrentCount == 0)
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
            if(this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }
}