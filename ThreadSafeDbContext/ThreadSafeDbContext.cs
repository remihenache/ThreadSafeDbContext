namespace Microsoft.EntityFrameworkCore.ThreadSafe;

public class ThreadSafeDbContext : DbContext
{
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

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
        this.semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeDbSet<TEntity>(base.Set<TEntity>(), this.semaphoreSlim);
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }


    public override async Task<Int32> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }

    public override Int32 SaveChanges()
    {
        this.semaphoreSlim.Wait();
        try
        {
            return base.SaveChanges();
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}