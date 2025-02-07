using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Internals;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal sealed class ThreadSafeInternalDbSet<TEntity> :
    InternalDbSet<TEntity>,
    IAsyncEnumerable<TEntity>,
    IOrderedQueryable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadSafeInternalDbSet(DbContext context, string entityName, SemaphoreSlim semaphoreSlim)
        : base(context, entityName)
    {
        _semaphoreSlim = semaphoreSlim;
    }


    public override LocalView<TEntity> Local => SafeExecute(() => base.Local);

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        return SafeExecute(() => base.Add(entity));
    }

    public override EntityEntry<TEntity> Attach(TEntity entity)
    {
        return SafeExecute(() => base.Attach(entity));
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        return SafeExecute(() => base.Update(entity));
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = new())
    {
        return SafeExecuteValueAsync(() => base.AddAsync(entity, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        return SafeExecute(() => base.Remove(entity));
    }

    public override void AddRange(params TEntity[] entities)
    {
        SafeExecute(() => base.AddRange(entities));
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => base.AddRange(entities));
    }

    public override void AttachRange(params TEntity[] entities)
    {
        SafeExecute(() => base.AttachRange(entities));
    }

    public override void AttachRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => base.AttachRange(entities));
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        SafeExecute(() => base.RemoveRange(entities));
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => base.RemoveRange(entities));
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        SafeExecute(() => base.UpdateRange(entities));
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => base.UpdateRange(entities));
    }

    public override Task AddRangeAsync(params TEntity[] entities)
    {
        return SafeExecuteAsync(() => base.AddRangeAsync(entities));
    }

    public override Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => base.AddRangeAsync(entities, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Entry(TEntity entity)
    {
        return SafeExecute(() => base.Entry(entity));
    }

    public override TEntity? Find(params object?[]? keyValues)
    {
        return SafeExecute(() => base.Find(keyValues));
    }

    public override ValueTask<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
    {
        return SafeExecuteValueAsync(() => base.FindAsync(keyValues, cancellationToken), cancellationToken);
    }

    public override ValueTask<TEntity?> FindAsync(params object?[]? keyValues)
    {
        return SafeExecuteValueAsync(() => base.FindAsync(keyValues));
    }


    private void SafeExecute(Action func)
    {
        _semaphoreSlim.Wait();
        try
        {
            func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private T SafeExecute<T>(Func<T> func)
    {
        _semaphoreSlim.Wait();
        try
        {
            return func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async ValueTask<T> SafeExecuteValueAsync<T>(Func<ValueTask<T>> func,
        CancellationToken cancellationToken = default)
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

    #region Queryable

    public override IQueryable<TEntity> AsQueryable()
    {
        return new ThreadSafeEntityQueryable<TEntity>((Provider as IAsyncQueryProvider)!, EntityType, _semaphoreSlim);
    }

    private IQueryProvider? _queryProvider;

    public IQueryProvider Provider
    {
        get
        {
            if (_queryProvider != null)
                return _queryProvider;

            var field = typeof(InternalDbSet<TEntity>).GetProperty("EntityQueryable", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var entityQueryable = (field.GetValue(this) as EntityQueryable<TEntity>)!;
            _queryProvider = new ThreadSafeEntityQueryProvider(entityQueryable.Provider, _semaphoreSlim);
            return _queryProvider;
        }
    }

    public override IAsyncEnumerable<TEntity> AsAsyncEnumerable()
    {
        return new ThreadSafeEntityQueryable<TEntity>((Provider as IAsyncQueryProvider)!, EntityType, _semaphoreSlim);
    }

    public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new ThreadSafeEntityQueryable<TEntity>((Provider as IAsyncQueryProvider)!, EntityType, _semaphoreSlim).GetAsyncEnumerator(
            cancellationToken);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return new ThreadSafeEntityQueryable<TEntity>((Provider as IAsyncQueryProvider)!, EntityType, _semaphoreSlim).GetEnumerator();
    }

    #endregion
}