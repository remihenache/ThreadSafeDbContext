using System.Collections;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

namespace Microsoft.EntityFrameworkCore.ThreadSafe;

internal sealed class ThreadSafeDbSet<TEntity> :
    DbSet<TEntity>,
    IAsyncEnumerable<TEntity>,
    IOrderedQueryable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim semaphoreSlim;
    private readonly DbSet<TEntity> set;

    public ThreadSafeDbSet(DbSet<TEntity> set, SemaphoreSlim semaphoreSlim)
    {
        this.set = set;
        this.semaphoreSlim = semaphoreSlim;
    }

    public override IEntityType EntityType => set.EntityType;

    public override LocalView<TEntity> Local => SafeExecute(() => set.Local);

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        return SafeExecute(() => set.Add(entity));
    }

    public override EntityEntry<TEntity> Attach(TEntity entity)
    {
        return SafeExecute(() => set.Attach(entity));
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        return SafeExecute(() => set.Update(entity));
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = new())
    {
        return SafeExecuteValueAsync(() => set.AddAsync(entity, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        return SafeExecute(() => set.Remove(entity));
    }

    public override void AddRange(params TEntity[] entities)
    {
        SafeExecute(() => set.AddRange(entities));
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => set.AddRange(entities));
    }

    public override void AttachRange(params TEntity[] entities)
    {
        SafeExecute(() => set.AttachRange(entities));
    }

    public override void AttachRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => set.AttachRange(entities));
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        SafeExecute(() => set.RemoveRange(entities));
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => set.RemoveRange(entities));
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        SafeExecute(() => set.UpdateRange(entities));
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        SafeExecute(() => set.UpdateRange(entities));
    }

    public override Task AddRangeAsync(params TEntity[] entities)
    {
        return SafeExecuteAsync(() => set.AddRangeAsync(entities));
    }

    public override Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new())
    {
        return SafeExecuteAsync(() => set.AddRangeAsync(entities, cancellationToken), cancellationToken);
    }

    public override EntityEntry<TEntity> Entry(TEntity entity)
    {
        return SafeExecute(() => set.Entry(entity));
    }

    public override TEntity? Find(params object?[]? keyValues)
    {
        return SafeExecute(() => set.Find(keyValues));
    }

    public override ValueTask<TEntity?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
    {
        return SafeExecuteValueAsync(() => set.FindAsync(keyValues, cancellationToken), cancellationToken);
    }

    public override ValueTask<TEntity?> FindAsync(params object?[]? keyValues)
    {
        return SafeExecuteValueAsync(() => set.FindAsync(keyValues));
    }


    private void SafeExecute(Action func)
    {
        semaphoreSlim.Wait();
        try
        {
            func();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private T SafeExecute<T>(Func<T> func)
    {
        semaphoreSlim.Wait();
        try
        {
            return func();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private async ValueTask<T> SafeExecuteValueAsync<T>(Func<ValueTask<T>> func,
        CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await func();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private async Task SafeExecuteAsync(Func<Task> func, CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await func();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    #region Queryable

    public Type ElementType => (set as IQueryable).ElementType;
    public Expression Expression => (set as IQueryable).Expression;

    public override IQueryable<TEntity> AsQueryable()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(set.AsQueryable(), semaphoreSlim);
    }

    public override IAsyncEnumerable<TEntity> AsAsyncEnumerable()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(set.AsQueryable(), semaphoreSlim);
    }

    public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new ThreadSafeAsyncQueryable<TEntity>(set.AsQueryable(), semaphoreSlim).GetAsyncEnumerator(
            cancellationToken);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IQueryProvider Provider =>
        new ThreadSafeAsyncQueryable<TEntity>(set.AsQueryable(), semaphoreSlim).Provider;

    public IEnumerator<TEntity> GetEnumerator()
    {
        return new ThreadSafeAsyncQueryable<TEntity>(set.AsQueryable(), semaphoreSlim).GetEnumerator();
    }

    #endregion
}