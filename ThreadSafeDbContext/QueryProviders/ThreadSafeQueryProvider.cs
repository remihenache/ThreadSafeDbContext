using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ThreadSafeDbContext.QueryProviders;

internal sealed class ThreadSafeQueryProvider : IAsyncQueryProvider
{
    private readonly IQueryProvider _queryProvider;
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadSafeQueryProvider(
        IQueryProvider queryProvider,
        SemaphoreSlim semaphoreSlim)
    {
        this._queryProvider = queryProvider;
        this._semaphoreSlim = semaphoreSlim;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new ThreadSafeQueryable(this._queryProvider.CreateQuery(expression), this._semaphoreSlim);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new ThreadSafeQueryable<TElement>(this._queryProvider.CreateQuery<TElement>(expression),
            this._semaphoreSlim);
    }

    public Object? Execute(Expression expression)
    {
        try
        {
            this._semaphoreSlim.Wait();
            return this._queryProvider.Execute(expression);
        }
        finally
        {
            if (this._semaphoreSlim.CurrentCount == 0)
                this._semaphoreSlim.Release();
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        try
        {
            this._semaphoreSlim.Wait();
            return this._queryProvider.Execute<TResult>(expression);
        }
        finally
        {
            if (this._semaphoreSlim.CurrentCount == 0)
                this._semaphoreSlim.Release();
        }
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        try
        {
            this._semaphoreSlim.Wait();
            return (this._queryProvider as IAsyncQueryProvider)!.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            if (this._semaphoreSlim.CurrentCount == 0)
                this._semaphoreSlim.Release();
        }
    }
}