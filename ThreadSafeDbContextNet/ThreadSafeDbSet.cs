using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkNet.ThreadSafe.QueryProviders;

namespace Microsoft.EntityFrameworkNet.ThreadSafe;

internal sealed class ThreadSafeDbSet<TEntity> :
    DbSet<TEntity>,
    IEnumerable<TEntity>,
    IOrderedQueryable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly DbSet<TEntity> _set;

    public ThreadSafeDbSet(DbSet<TEntity> set, SemaphoreSlim semaphoreSlim)
    {
        _set = set;
        _semaphoreSlim = semaphoreSlim;
    }

    public override IEntityType EntityType => _set.EntityType;

    public override LocalView<TEntity> Local => SafeExecute(() => _set.Local);

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        return SafeExecute(() => _set.Add(entity));
    }

    public override EntityEntry<TEntity> Attach(TEntity entity)
    {
        return SafeExecute(() => _set.Attach(entity));
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        return SafeExecute(() => _set.Update(entity));
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = new())
    {
        return SafeExecuteValueAsync(() => _set.AddAsync(entity, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        return SafeExecute(() => _set.Remove(entity));
    }

    public override void AddRange(params TEntity[] entities)
    {
        SafeExecute(() => _set.AddRange(entities));
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => _set.AddRange(entities));
    }

    public override void AttachRange(params TEntity[] entities)
    {
        SafeExecute(() => _set.AttachRange(entities));
    }

    public override void AttachRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => _set.AttachRange(entities));
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        SafeExecute(() => _set.RemoveRange(entities));
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => _set.RemoveRange(entities));
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        SafeExecute(() => _set.UpdateRange(entities));
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => _set.UpdateRange(entities));
    }

    public override Task AddRangeAsync(params TEntity[] entities)
    {
        return SafeExecuteAsync(() => _set.AddRangeAsync(entities));
    }

    public override Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => _set.AddRangeAsync(entities, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Entry(TEntity entity)
    {
        return SafeExecute(() => _set.Entry(entity));
    }

    public override TEntity? Find(params object?[]? keyValues)
    {
        return SafeExecute(() => _set.Find(keyValues));
    }

    public override ValueTask<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
    {
        return SafeExecuteValueAsync(() => _set.FindAsync(keyValues, cancellationToken), cancellationToken);
    }

    public override ValueTask<TEntity?> FindAsync(params object?[]? keyValues)
    {
        return SafeExecuteValueAsync(() => _set.FindAsync(keyValues));
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

    public Type ElementType => (_set as IQueryable).ElementType;
    public Expression Expression => (_set as IQueryable).Expression;

    public override IQueryable<TEntity> AsQueryable()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim);
    }

    public override IAsyncEnumerable<TEntity> AsAsyncEnumerable()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim);
    }

    public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new ThreadSafeAsyncQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim).GetAsyncEnumerator(
            cancellationToken);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IQueryProvider Provider =>
        new ThreadSafeAsyncQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim).Provider;
    
    public IEnumerator<TEntity> GetEnumerator()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim).GetEnumerator();
    }

    #endregion
    
    public void ResetState()
    {
        (_set as IResettableService)?.ResetState();
    }

    public async Task ResetStateAsync(CancellationToken cancellationToken = new())
    {
       if(_set is IResettableService resettableService)
         await resettableService.ResetStateAsync(cancellationToken);
    }
}
