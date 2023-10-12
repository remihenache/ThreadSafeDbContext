using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal sealed class ThreadSafeQueryProvider : IAsyncQueryProvider
{
    private readonly IQueryProvider queryProvider;
    private readonly SemaphoreSlim semaphoreSlim;

    public ThreadSafeQueryProvider(
        IQueryProvider queryProvider,
        SemaphoreSlim semaphoreSlim)
    {
        this.queryProvider = queryProvider;
        this.semaphoreSlim = semaphoreSlim;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new ThreadSafeQueryable(this.queryProvider.CreateQuery(expression), this.semaphoreSlim);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new ThreadSafeQueryable<TElement>(this.queryProvider.CreateQuery<TElement>(expression),
            this.semaphoreSlim);
    }

    public Object? Execute(Expression expression)
    {
        try
        {
            this.semaphoreSlim.Wait();
            return this.queryProvider.Execute(expression);
        }
        finally
        {
            if (this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        try
        {
            this.semaphoreSlim.Wait();
            return this.queryProvider.Execute<TResult>(expression);
        }
        finally
        {
            if (this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        try
        {
            this.semaphoreSlim.Wait();
            return (this.queryProvider as IAsyncQueryProvider)!.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            if (this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }
}