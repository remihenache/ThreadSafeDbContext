using System.Collections;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

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

    public new IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim);
    }
}

internal sealed class ThreadSafeAsyncQueryable<T> : ThreadSafeQueryable<T>, IAsyncEnumerable<T>
{
    public ThreadSafeAsyncQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim)
        : base(set, semaphoreSlim)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new ThreadSafeAsyncEnumerator<T>(
            (Set as IAsyncEnumerable<T>)!.GetAsyncEnumerator(cancellationToken), SemaphoreSlim);
    }
}