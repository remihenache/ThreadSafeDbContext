using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.ThreadSafe.Internals;

namespace Microsoft.EntityFrameworkCore.ThreadSafe;

public class ThreadSafeDbContext : DbContext
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    
    public static DbContext Wrap<T>(DbContext context)
        where T: DbContext
    {
        var existingOptions = context.GetService<IDbContextOptions>() as DbContextOptions<T>;
        var newOptions = new DbContextOptionsBuilder<T>(existingOptions!)
            .EnableThreadSafetyChecks(false) 
            .Options;

        var dbContext = (T)Activator.CreateInstance(typeof(T), newOptions)!;
        dbContext.Database.CanConnect();
        return new ThreadSafeDbContextWrapper(dbContext);
    }
    
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
            return new ThreadSafeInternalDbSet<TEntity>(this, typeof(TEntity).FullName!, _semaphoreSlim);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override DbSet<TEntity> Set<TEntity>(string name)
    {
        _semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeInternalDbSet<TEntity>(this, name, _semaphoreSlim);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }


    public override object? Find(Type entityType, params object?[]? keyValues)
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.Find(entityType, keyValues);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override async ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            return await base.FindAsync(entityType, keyValues);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override async ValueTask<object?> FindAsync(Type entityType, object?[]? keyValues,
        CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await base.FindAsync(entityType, keyValues, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override void Dispose()
    {
        _semaphoreSlim.Dispose();
        base.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _semaphoreSlim.Dispose();
        return base.DisposeAsync();
    }
    
    public override IQueryable<TResult> FromExpression<TResult>(Expression<Func<IQueryable<TResult>>> expression)
    {
        var queryable = base.FromExpression(expression);
        return new ThreadSafeEntityQueryable<TResult>((queryable.Provider as IAsyncQueryProvider)!, queryable.Expression, _semaphoreSlim);
    }
}