using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSafeDbContextNet.QueryProviders;

internal class ThreadSafeEnumerator : IEnumerator, IDbAsyncEnumerator
{
    protected readonly IDbAsyncEnumerator? DbEnumerator;
    protected readonly IEnumerator? Enumerator;
    protected readonly SemaphoreSlim SemaphoreSlim;
    protected readonly ThrowingMonitorProxy ThrowingMonitor;

    public ThreadSafeEnumerator(IEnumerator enumerator, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        Enumerator = enumerator;
        SemaphoreSlim = semaphoreSlim;
        ThrowingMonitor = throwingMonitor;
    }

    public ThreadSafeEnumerator(IDbAsyncEnumerator enumerator, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
    {
        DbEnumerator = enumerator;
        SemaphoreSlim = semaphoreSlim;
        ThrowingMonitor = throwingMonitor;
    }

    public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);
        ThrowingMonitor.Erase();
        try
        {
            return Enumerator?.MoveNext() ?? await DbEnumerator!.MoveNextAsync(cancellationToken);
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public void Dispose()
    {
        if (DbEnumerator is null) return;
        SemaphoreSlim.Wait();
        ThrowingMonitor.Erase();
        try
        {
            DbEnumerator.Dispose();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public bool MoveNext()
    {
        SemaphoreSlim.Wait();
        ThrowingMonitor.Erase();
        try
        {
            return Enumerator?.MoveNext() ?? DbEnumerator!.MoveNextAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public void Reset()
    {
        SemaphoreSlim.Wait();
        ThrowingMonitor.Erase();
        try
        {
            Enumerator?.Reset();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public object Current => Enumerator?.Current ?? DbEnumerator!.Current;
}

internal sealed class ThreadSafeEnumerator<T> : ThreadSafeEnumerator, IEnumerator<T>, IDbAsyncEnumerator<T>
{
    public ThreadSafeEnumerator(IEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
        : base(enumerator, semaphoreSlim, throwingMonitor)
    {
    }

    public ThreadSafeEnumerator(IDbAsyncEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim, ThrowingMonitorProxy throwingMonitor)
        : base(enumerator, semaphoreSlim, throwingMonitor)
    {
    }

    public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);
        ThrowingMonitor.Erase();
        try
        {
            return Enumerator?.MoveNext() ?? await DbEnumerator!.MoveNextAsync(cancellationToken);
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public new T Current => ((T)Enumerator?.Current ?? (T)DbEnumerator!.Current)!;


    public void Dispose()
    {
        if (DbEnumerator is null) return;
        SemaphoreSlim.Wait();
        ThrowingMonitor.Erase();
        try
        {
            DbEnumerator.Dispose();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    T IEnumerator<T>.Current => ((T)Enumerator?.Current ?? (T)DbEnumerator!.Current)!;
}