using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal sealed class ThreadSafeQueryProvider : IAsyncQueryProvider
{
    private readonly IQueryProvider _queryProvider;
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadSafeQueryProvider(
        IQueryProvider queryProvider,
        SemaphoreSlim semaphoreSlim)
    {
        _queryProvider = queryProvider;
        _semaphoreSlim = semaphoreSlim;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new ThreadSafeQueryable(_queryProvider.CreateQuery(expression), _semaphoreSlim);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        if (_queryProvider is IAsyncQueryProvider)
            return new ThreadSafeAsyncQueryable<TElement>(_queryProvider.CreateQuery<TElement>(expression),
                _semaphoreSlim);
        return new ThreadSafeQueryable<TElement>(_queryProvider.CreateQuery<TElement>(expression),
            _semaphoreSlim);
    }

    public object? Execute(Expression expression)
    {
        _semaphoreSlim.Wait();
        try
        {
            return _queryProvider.Execute(expression);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public TResult Execute<TResult>(Expression expression)
    {
        _semaphoreSlim.Wait();
        try
        {
            return _queryProvider.Execute<TResult>(expression);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        _semaphoreSlim.Wait(cancellationToken);
        try
        {
            return (_queryProvider as IAsyncQueryProvider)!.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}