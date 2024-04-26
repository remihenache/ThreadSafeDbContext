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
        semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeDbSet<TEntity>(base.Set<TEntity>(), semaphoreSlim);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override DbSet<TEntity> Set<TEntity>(string name)
    {
        semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeDbSet<TEntity>(base.Set<TEntity>(name), semaphoreSlim);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        semaphoreSlim.Wait();
        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }


    public override object? Find(Type entityType, params object?[]? keyValues)
    {
        semaphoreSlim.Wait();
        try
        {
            return base.Find(entityType, keyValues);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override async ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            return await base.FindAsync(entityType, keyValues);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override async ValueTask<object?> FindAsync(Type entityType, object?[]? keyValues,
        CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await base.FindAsync(entityType, keyValues, cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public override void Dispose()
    {
        semaphoreSlim.Dispose();
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        semaphoreSlim.Dispose();
        return base.DisposeAsync();
    }
}