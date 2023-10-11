using System.Collections;
using System.Linq.Expressions;

namespace ThreadSafeDbContext.QueryProviders;

internal class ThreadSafeQueryable : IQueryable
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
        try
        {
            this.SemaphoreSlim.Wait();
            return this.Set.GetEnumerator();
        }
        finally
        {
            if (this.SemaphoreSlim.CurrentCount == 0) this.SemaphoreSlim.Release();
        }
    }

    public Type ElementType => this.Set.ElementType;

    public Expression Expression => this.Set.Expression;

    public IQueryProvider Provider => new ThreadSafeQueryProvider(this.Set.Provider, this.SemaphoreSlim);
}

internal sealed class ThreadSafeQueryable<T> : ThreadSafeQueryable, IQueryable<T>, IAsyncEnumerable<T>
{
    public ThreadSafeQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim)
        : base(set, semaphoreSlim)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        try
        {
            this.SemaphoreSlim.Wait(cancellationToken);
            return (this.Set as IAsyncEnumerable<T>)!.GetAsyncEnumerator(cancellationToken);
        }
        finally
        {
            if (this.SemaphoreSlim.CurrentCount == 0)
                this.SemaphoreSlim.Release();
        }
    }

    public new IEnumerator<T> GetEnumerator()
    {
        try
        {
            this.SemaphoreSlim.Wait();
            return (this.Set as IQueryable<T>)!.GetEnumerator();
        }
        finally
        {
            if (this.SemaphoreSlim.CurrentCount == 0)
                this.SemaphoreSlim.Release();
        }
    }
}