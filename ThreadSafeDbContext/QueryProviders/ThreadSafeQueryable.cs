using System.Collections;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal class ThreadSafeQueryable : IOrderedQueryable
{
    protected readonly SemaphoreSlim SemaphoreSlim;
    protected readonly IQueryable Set;

    public ThreadSafeQueryable(IQueryable set, SemaphoreSlim semaphoreSlim)
    {
        this.Set = set;
        this.SemaphoreSlim = semaphoreSlim;
    }


    public IEnumerator GetEnumerator()
    {
        return new ThreadSafeEnumerator(this.Set.GetEnumerator(), this.SemaphoreSlim);
    }

    public Type ElementType => this.Set.ElementType;

    public Expression Expression => this.Set.Expression;

    public IQueryProvider Provider => new ThreadSafeQueryProvider(this.Set.Provider, this.SemaphoreSlim);
}

internal sealed class ThreadSafeQueryable<T> : ThreadSafeQueryable, IAsyncEnumerable<T>, IOrderedQueryable<T>
{
    public ThreadSafeQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim)
        : base(set, semaphoreSlim)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new ThreadSafeEnumerator<T>((this.Set as IQueryable<T>)!.GetEnumerator(), this.SemaphoreSlim);
    }

    public new IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>((this.Set as IQueryable<T>)!.GetEnumerator(), this.SemaphoreSlim);
    }
}