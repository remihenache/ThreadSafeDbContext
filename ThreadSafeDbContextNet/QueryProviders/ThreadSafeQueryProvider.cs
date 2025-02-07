using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSafeDbContextNet.QueryProviders;

internal sealed class ThreadSafeQueryProvider : IQueryProvider, IDbAsyncQueryProvider
{
    private readonly IQueryProvider _queryProvider;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly ThrowingMonitorProxy ThrowingMonitor;

    public ThreadSafeQueryProvider(IQueryProvider queryProvider,
        SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        _queryProvider = queryProvider;
        _semaphoreSlim = semaphoreSlim;
        ThrowingMonitor = throwingMonitor;
    }

    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
    {
        _semaphoreSlim.Wait(cancellationToken);
        ThrowingMonitor?.Erase();
        try
        {
            return (_queryProvider as IDbAsyncQueryProvider)!.ExecuteAsync(expression, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        _semaphoreSlim.Wait(cancellationToken);
        ThrowingMonitor?.Erase();
        try
        {
            return (_queryProvider as IDbAsyncQueryProvider)!.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new ThreadSafeQueryable(_queryProvider.CreateQuery(expression), _semaphoreSlim, ThrowingMonitor);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new ThreadSafeQueryable<TElement>(_queryProvider.CreateQuery<TElement>(expression),
            _semaphoreSlim, ThrowingMonitor);
    }

    public object? Execute(Expression expression)
    {
        _semaphoreSlim.Wait();
        ThrowingMonitor?.Erase();
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
        ThrowingMonitor?.Erase();
        try
        {
            return _queryProvider.Execute<TResult>(expression);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}