using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.ThreadSafe.Internals;

namespace Microsoft.EntityFrameworkCore.ThreadSafe;

internal class ThreadSafeDbContextWrapper : DbContext
{
    private readonly DbContext _context;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    internal ThreadSafeDbContextWrapper(DbContext context)
    {
        _context = context;
    }

    public override DatabaseFacade Database => _context.Database;
    public override ChangeTracker ChangeTracker => _context.ChangeTracker;
    public override IModel Model => _context.Model;
    public override DbContextId ContextId => _context.ContextId;

    public override DbSet<TEntity> Set<TEntity>()
    {
        _semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeInternalDbSet<TEntity>(_context, typeof(TEntity).FullName!, _semaphoreSlim);
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
            return new ThreadSafeInternalDbSet<TEntity>(_context, name, _semaphoreSlim);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableThreadSafetyChecks(false);
        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges()
    {
        return SafeExecute(() => _context.SaveChanges());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
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
            return await _context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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
            return _context.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }


    public override void RemoveRange(IEnumerable<object> entities)
    {
        SafeExecute(() => _context.RemoveRange(entities));
    }

    public override object? Find(Type entityType, params object?[]? keyValues)
    {
        _semaphoreSlim.Wait();
        try
        {
            return _context.Find(entityType, keyValues);
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
            return await _context.FindAsync(entityType, keyValues);
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
            return await _context.FindAsync(entityType, keyValues, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override TEntity? Find<TEntity>(params object?[]? keyValues) where TEntity : class
    {
        return SafeExecute(() => _context.Find<TEntity>(keyValues));
    }

    public override ValueTask<TEntity?> FindAsync<TEntity>(params object?[]? keyValues) where TEntity : class
    {
        return SafeExecuteAsync(() => _context.FindAsync<TEntity>(keyValues));
    }

    public override ValueTask<TEntity?> FindAsync<TEntity>(object?[]? keyValues, CancellationToken cancellationToken) where TEntity : class
    {
        return SafeExecuteAsync(() => _context.FindAsync<TEntity>(keyValues, cancellationToken), cancellationToken);
    }

    public override IQueryable<TResult> FromExpression<TResult>(Expression<Func<IQueryable<TResult>>> expression)
    {
        var queryable = _context.FromExpression(expression);
        return new ThreadSafeEntityQueryable<TResult>((queryable.Provider as IAsyncQueryProvider)!, queryable.Expression, _semaphoreSlim);
    }

    public override string ToString()
    {
        return _context.ToString()!;
    }

    public override bool Equals(object? obj)
    {
        return _context.Equals(obj);
    }

    public override int GetHashCode()
    {
        return _context.GetHashCode();
    }

    public override void Dispose()
    {
        _semaphoreSlim.Dispose();
        _context.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _semaphoreSlim.Dispose();
        return _context.DisposeAsync();
    }

    public override EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
    {
        return SafeExecute(() => _context.Entry(entity));
    }

    public override EntityEntry Entry(object entity)
    {
        return SafeExecute(() => _context.Entry(entity));
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        return SafeExecute(() => _context.Add(entity));
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => _context.AddAsync(entity, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
    {
        return SafeExecute(() => _context.Attach(entity));
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        return SafeExecute(() => _context.Update(entity));
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        return SafeExecute(() => _context.Remove(entity));
    }

    public override EntityEntry Add(object entity)
    {
        return SafeExecute(() => _context.Add(entity));
    }

    public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => _context.AddAsync(entity, cancellationToken), cancellationToken);
    }

    public override EntityEntry Attach(object entity)
    {
        return SafeExecute(() => _context.Attach(entity));
    }

    public override EntityEntry Update(object entity)
    {
        return SafeExecute(() => _context.Update(entity));
    }

    public override EntityEntry Remove(object entity)
    {
        return SafeExecute(() => _context.Remove(entity));
    }

    public override void AddRange(params object[] entities)
    {
        SafeExecute(() => _context.AddRange(entities));
    }

    public override Task AddRangeAsync(params object[] entities)
    {
        return SafeExecuteAsync(() => _context.AddRangeAsync(entities));
    }

    public override void AttachRange(params object[] entities)
    {
        SafeExecute(() => _context.AttachRange(entities));
    }

    public override void UpdateRange(params object[] entities)
    {
        SafeExecute(() => _context.UpdateRange(entities));
    }

    public override void RemoveRange(params object[] entities)
    {
        SafeExecute(() => _context.RemoveRange(entities));
    }

    public override void AddRange(IEnumerable<object> entities)
    {
        SafeExecute(() => _context.AddRange(entities));
    }

    public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => _context.AddRangeAsync(entities, cancellationToken), cancellationToken);
    }

    public override void AttachRange(IEnumerable<object> entities)
    {
        SafeExecute(() => _context.AttachRange(entities));
    }

    public override void UpdateRange(IEnumerable<object> entities)
    {
        SafeExecute(() => _context.UpdateRange(entities));
    }


    private async Task SafeExecuteAsync(Func<Task> func, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async ValueTask<T> SafeExecuteAsync<T>(Func<ValueTask<T>> func, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void SafeExecute(Action func, CancellationToken cancellationToken = default)
    {
        _semaphoreSlim.Wait(cancellationToken);
        try
        {
            func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private T SafeExecute<T>(Func<T> func, CancellationToken cancellationToken = default)
    {
        _semaphoreSlim.Wait(cancellationToken);
        try
        {
            return func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}