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
        return new ThreadSafeQueryable(queryProvider.CreateQuery(expression), semaphoreSlim);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        if (queryProvider is IAsyncQueryProvider)
            return new ThreadSafeAsyncQueryable<TElement>(queryProvider.CreateQuery<TElement>(expression),
                semaphoreSlim);
        return new ThreadSafeQueryable<TElement>(queryProvider.CreateQuery<TElement>(expression),
            semaphoreSlim);
    }

    public object? Execute(Expression expression)
    {
        semaphoreSlim.Wait();
        try
        {
            return queryProvider.Execute(expression);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        semaphoreSlim.Wait();
        try
        {
            return queryProvider.Execute<TResult>(expression);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        semaphoreSlim.Wait(cancellationToken);
        try
        {
            return (queryProvider as IAsyncQueryProvider)!.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}