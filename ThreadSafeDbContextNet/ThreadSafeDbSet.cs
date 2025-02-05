using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
    IDbAsyncEnumerable,
    IDbAsyncEnumerable<TEntity>,
    IOrderedQueryable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly DbSet<TEntity> _set;

    public ThreadSafeDbSet(DbSet<TEntity> set, SemaphoreSlim semaphoreSlim)
    {
        _set = set;
        Local = _set.Local;
        _semaphoreSlim = semaphoreSlim;
    }

    public override TEntity Add(TEntity entity)
    {
        return SafeExecute(() => _set.Add(entity));
    }

    public override TEntity Attach(TEntity entity)
    {
        return SafeExecute(() => _set.Attach(entity));
    }

    public override TEntity Create()
    {
        return SafeExecute(() => _set.Create());
    }

    public override TDerivedEntity Create<TDerivedEntity>()
    {
        return SafeExecute(() => _set.Create<TDerivedEntity>());
    }

    public override DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters)
    {
        return SafeExecute(() => _set.SqlQuery(sql, parameters));
    }

    public override TEntity Remove(TEntity entity)
    {
        return SafeExecute(() => _set.Remove(entity));
    }

    public override Task<TEntity> FindAsync(CancellationToken cancellationToken, params object?[]? keyValues)
    {
        return SafeExecuteAsync(() => _set.FindAsync(cancellationToken, keyValues), cancellationToken);
    }

    public override Task<TEntity?> FindAsync(params object?[]? keyValues)
    {
        return SafeExecuteAsync(() => _set.FindAsync(keyValues));
    }

    public override IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
    {
        return SafeExecute(() => _set.AddRange(entities));
    }

    public override IEnumerable<TEntity> RemoveRange(IEnumerable<TEntity> entities)
    {
        return SafeExecute(() => _set.RemoveRange(entities));
    }

    public override TEntity? Find(params object?[]? keyValues)
    {
        return SafeExecute(() => _set.Find(keyValues));
    }

    public IDbAsyncEnumerator GetAsyncEnumerator()
    {
        return SafeExecute(() => ((IDbAsyncEnumerable) _set).GetAsyncEnumerator());
    }

    IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
    {
        return SafeExecute(() => ((IDbAsyncEnumerable<TEntity>) _set).GetAsyncEnumerator());
    }

    public override DbQuery<TEntity> AsNoTracking()
    {
        return SafeExecute(() => _set.AsNoTracking());
    }

    public override DbQuery<TEntity> Include(string path)
    {
        return SafeExecute(() => _set.Include(path));
    }

    public override ObservableCollection<TEntity> Local { get; }

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
    
    private async Task<T> SafeExecuteAsync<T>(Func<Task<T>> func,
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

    #region Queryable

    public Type ElementType => (_set as IQueryable).ElementType;
    public Expression Expression => (_set as IQueryable).Expression;

    public IQueryable<TEntity> AsQueryable()
    {
        return new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim);
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IQueryProvider Provider =>
        new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim).Provider;
    
    public IEnumerator<TEntity> GetEnumerator()
    {
        return new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim).GetEnumerator();
    }

    #endregion
}
