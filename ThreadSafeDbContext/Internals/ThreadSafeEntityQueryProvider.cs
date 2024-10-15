using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.Internals;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal sealed class ThreadSafeEntityQueryProvider : EntityQueryProvider
{
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadSafeEntityQueryProvider(
        IQueryProvider queryProvider,
        SemaphoreSlim semaphoreSlim)
        : base(GetQueryCompiler(queryProvider))
    {
        _semaphoreSlim = semaphoreSlim;
    }


    private static IQueryCompiler GetQueryCompiler(IQueryProvider queryProvider)
    {
        var field = typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance);
        return (IQueryCompiler)field!.GetValue(queryProvider)!;
    }

    private static MethodInfo? _genericCreateQueryMethod;
    private static MethodInfo GenericCreateQueryMethod
        => _genericCreateQueryMethod ??= typeof(ThreadSafeEntityQueryProvider)
            .GetMethod("CreateQuery", 1, BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(Expression) }, null)!;
    public override IQueryable CreateQuery(Expression expression)
    {
        return (IQueryable)GenericCreateQueryMethod
            .MakeGenericMethod(expression.Type)
            .Invoke(this, new object[] { expression })!;
    }

    public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        var baseQueryable = base.CreateQuery<TElement>(expression);
        return new ThreadSafeEntityQueryable<TElement>((baseQueryable.Provider as IAsyncQueryProvider)!, expression, _semaphoreSlim);
    }

    public override object Execute(Expression expression)
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.Execute(expression);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override TResult Execute<TResult>(Expression expression)
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.Execute<TResult>(expression);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        _semaphoreSlim.Wait(cancellationToken);
        try
        {
            return base.ExecuteAsync<TResult>(expression, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}