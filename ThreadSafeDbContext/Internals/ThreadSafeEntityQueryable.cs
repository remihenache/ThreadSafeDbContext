using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Internals;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal class ThreadSafeEntityQueryable<T> : EntityQueryable<T>, IAsyncEnumerable<T>, IOrderedQueryable, IListSource
{
    private readonly SemaphoreSlim _semaphoreSlim;
    public ThreadSafeEntityQueryable(IAsyncQueryProvider queryProvider, IEntityType entityType, SemaphoreSlim semaphoreSlim)
    :base(queryProvider, entityType)
    {
        _semaphoreSlim = semaphoreSlim;
    }
    public ThreadSafeEntityQueryable(IAsyncQueryProvider queryProvider, Expression expression, SemaphoreSlim semaphoreSlim)
        : base(queryProvider, expression)
    {
        _semaphoreSlim = semaphoreSlim;
    }

    public override IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>(base.GetEnumerator(), _semaphoreSlim);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>(base.GetEnumerator(), _semaphoreSlim);
    }


    public override IQueryProvider Provider => new ThreadSafeEntityQueryProvider(base.Provider, _semaphoreSlim);

    public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
    {
        return new ThreadSafeAsyncEnumerator<T>(base.GetAsyncEnumerator(cancellationToken), _semaphoreSlim);
    }

    public bool ContainsListCollection => false;

    public IList GetList()
    {
        return new List<T>(new ThreadSafeQueryable(this, _semaphoreSlim).OfType<T>());
    }
    
}
