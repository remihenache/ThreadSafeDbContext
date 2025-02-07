using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using ThreadSafeDbContextNet.QueryProviders;

namespace ThreadSafeDbContextNet;

internal sealed class ThreadSafeDbSet<TEntity> :
    DbSet<TEntity>,
    IDbAsyncEnumerable<TEntity>,
    IOrderedQueryable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly DbSet<TEntity> _set;
    private readonly ThrowingMonitorProxy _throwingMonitor;

    public ThreadSafeDbSet(DbSet<TEntity> set, SemaphoreSlim semaphoreSlim)
    {
        _set = set;
        Local = _set.Local;
        _semaphoreSlim = semaphoreSlim;

        _throwingMonitor = GetThrowingMonitor();
    }

    public override ObservableCollection<TEntity> Local { get; }

    public IDbAsyncEnumerator GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator(((IDbAsyncEnumerable)_set).GetAsyncEnumerator(), _semaphoreSlim, _throwingMonitor);
    }

    IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator<TEntity>(((IDbAsyncEnumerable<TEntity>)_set).GetAsyncEnumerator(), _semaphoreSlim, _throwingMonitor);
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

    public override DbQuery<TEntity> AsNoTracking()
    {
        return DbQueryProxyFactory.Create(_set.AsNoTracking(), _semaphoreSlim, _throwingMonitor);
    }

    public override DbQuery<TEntity> Include(string path)
    {
        return DbQueryProxyFactory.Create(_set.Include(path), _semaphoreSlim, _throwingMonitor);
    }

    private void SafeExecute(Action func)
    {
        _semaphoreSlim.Wait();
        try
        {
            _throwingMonitor.Erase();
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
            _throwingMonitor.Erase();
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
            _throwingMonitor.Erase();
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
        _throwingMonitor.Erase();
        try
        {
            return await func();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private ThrowingMonitorProxy GetThrowingMonitor()
    {
        // Accéder à _internalQuery
        var internalQueryField = _set.GetType()
            .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance);
        if (internalQueryField == null) throw new InvalidOperationException("Field '_internalQuery' not found in DbSet.");

        var internalQuery = internalQueryField.GetValue(_set);
        if (internalQuery == null) throw new InvalidOperationException("InternalQuery is null.");

        // Accéder à ObjectQuery
        var objectQueryProperty = internalQuery.GetType()
            .GetProperty("ObjectQuery");
        if (objectQueryProperty == null) throw new InvalidOperationException("Property 'ObjectQuery' not found in InternalQuery.");

        var objectQuery = objectQueryProperty.GetValue(internalQuery) as ObjectQuery;
        if (objectQuery == null) throw new InvalidOperationException("ObjectQuery is null.");
        var contextField = objectQuery.GetType()
            .GetProperty("Context");
        if (contextField == null) throw new InvalidOperationException("_context is null.");
        var context = contextField.GetValue(objectQuery);
        if (context == null) throw new InvalidOperationException("Context is null.");
        var throwingMonitorField = context.GetType()
            .GetField("_asyncMonitor", BindingFlags.NonPublic | BindingFlags.Instance);
        //var fakeMonitor = MonitorHack.CreateFakeThrowingMonitor();
        var originalMonitor = throwingMonitorField.GetValue(context);

        if (originalMonitor == null) throw new InvalidOperationException("Current '_asyncMonitor' instance is null.");

        return new ThrowingMonitorProxy(originalMonitor);
    }

    #region Queryable

    public Type ElementType => (_set as IQueryable).ElementType;
    public Expression Expression => (_set as IQueryable).Expression;

    public IQueryable<TEntity> AsQueryable()
    {
        return new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim, _throwingMonitor);
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IQueryProvider Provider =>
        new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim, _throwingMonitor).Provider;

    public IEnumerator<TEntity> GetEnumerator()
    {
        return new ThreadSafeQueryable<TEntity>(_set.AsQueryable(), _semaphoreSlim, _throwingMonitor).GetEnumerator();
    }

    #endregion
}

internal class ThreadSafeDbQueryProxy<T> : IInterceptor
{
    private readonly DbQuery<T> _innerQuery;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly ThrowingMonitorProxy _throwingMonitor;

    public ThreadSafeDbQueryProxy(DbQuery<T> innerQuery, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        _innerQuery = innerQuery;
        _semaphoreSlim = semaphoreSlim;
        _throwingMonitor = throwingMonitor;
    }

    public void Intercept(IInvocation invocation)
    {
        _semaphoreSlim.Wait();
        try
        {
            invocation.Proceed();
            invocation.ReturnValue = invocation.ReturnValue switch
            {
                DbQuery<T> dbQuery => DbQueryProxyFactory.Create(dbQuery, _semaphoreSlim, _throwingMonitor),
                IDbAsyncEnumerator<T> asyncEnumerator => new ThreadSafeEnumerator<T>(asyncEnumerator, _semaphoreSlim, _throwingMonitor),
                IEnumerator<T> enumerator => new ThreadSafeEnumerator<T>(enumerator, _semaphoreSlim, _throwingMonitor),
                _ => invocation.ReturnValue
            };
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}

public static class DbQueryProxyFactory
{
    private static readonly ProxyGenerator Generator = new();

    public static DbQuery<T> Create<T>(DbQuery<T> innerQuery, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        return Generator.CreateClassProxyWithTarget(innerQuery, new ThreadSafeDbQueryProxy<T>(innerQuery, semaphoreSlim, throwingMonitor));
    }
}