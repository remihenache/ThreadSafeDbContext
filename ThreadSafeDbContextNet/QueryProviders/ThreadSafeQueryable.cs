using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Microsoft.EntityFrameworkNet.ThreadSafe.QueryProviders;

internal class ThreadSafeQueryable : IOrderedQueryable
{
    protected readonly SemaphoreSlim SemaphoreSlim;
    protected readonly IQueryable Set;

    public ThreadSafeQueryable(IQueryable set, SemaphoreSlim semaphoreSlim)
    {
        Set = set;
        SemaphoreSlim = semaphoreSlim;
    }


    public IEnumerator GetEnumerator()
    {
        return new ThreadSafeEnumerator(Set.GetEnumerator(), SemaphoreSlim);
    }

    public Type ElementType => Set.ElementType;

    public Expression Expression => Set.Expression;

    public IQueryProvider Provider => new ThreadSafeQueryProvider(Set.Provider, SemaphoreSlim);


}

internal class ThreadSafeQueryable<T> : ThreadSafeQueryable, IOrderedQueryable<T>
{
    public ThreadSafeQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim)
        : base(set, semaphoreSlim)
    {
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public new IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim);
    }
}
