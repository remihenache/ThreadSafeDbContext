using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace ThreadSafeDbContextNet.QueryProviders;

internal class ThreadSafeQueryable : IOrderedQueryable, IDbAsyncEnumerable
{
    protected readonly SemaphoreSlim SemaphoreSlim;
    protected readonly IQueryable Set;
    protected readonly ThrowingMonitorProxy ThrowingMonitor;

    public ThreadSafeQueryable(IQueryable set, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        Set = set;
        SemaphoreSlim = semaphoreSlim;
        ThrowingMonitor = throwingMonitor;
    }


    public IDbAsyncEnumerator GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator(Set.GetEnumerator(), SemaphoreSlim, ThrowingMonitor);
    }


    public IEnumerator GetEnumerator()
    {
        return new ThreadSafeEnumerator(Set.GetEnumerator(), SemaphoreSlim, ThrowingMonitor);
    }

    public Type ElementType => Set.ElementType;

    public Expression Expression => Set.Expression;

    public IQueryProvider Provider => new ThreadSafeQueryProvider(Set.Provider, SemaphoreSlim, ThrowingMonitor);
}

internal class ThreadSafeQueryable<T> : ThreadSafeQueryable, IOrderedQueryable<T>, IDbAsyncEnumerable<T>
{
    public ThreadSafeQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
        : base(set, semaphoreSlim, throwingMonitor)
    {
    }

    public IDbAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim, ThrowingMonitor);
    }

    IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public new IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim, ThrowingMonitor);
    }
}