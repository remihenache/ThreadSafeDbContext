using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkNet.ThreadSafe.QueryProviders;

internal class ThreadSafeEnumerator : IEnumerator, IDbAsyncEnumerator
{
    protected readonly IEnumerator Enumerator;
    protected readonly SemaphoreSlim SemaphoreSlim;

    public ThreadSafeEnumerator(IEnumerator enumerator, SemaphoreSlim semaphoreSlim)
    {
        Enumerator = enumerator;
        SemaphoreSlim = semaphoreSlim;
    }

    public bool MoveNext()
    {
        SemaphoreSlim.Wait();
        try
        {
            return Enumerator.MoveNext();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public void Reset()
    {
        SemaphoreSlim.Wait();
        try
        {
            Enumerator.Reset();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return Enumerator.MoveNext();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public object Current => Enumerator.Current!;
    public void Dispose()
    {
        if (Enumerator is not IDisposable disposable) return;
        SemaphoreSlim.Wait();
        try
        {
            disposable.Dispose();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }
}

internal sealed class ThreadSafeEnumerator<T> : ThreadSafeEnumerator, IEnumerator<T>, IDbAsyncEnumerator<T>
{
    public ThreadSafeEnumerator(IEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim)
        : base(enumerator, semaphoreSlim)
    {
    }


    public void Dispose()
    {
        if (Enumerator is not IDisposable disposable) return;
        SemaphoreSlim.Wait();
        try
        {
            disposable.Dispose();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        await SemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await ((IDbAsyncEnumerator<T>) Enumerator).MoveNextAsync(cancellationToken);
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    public new T Current => ((T) Enumerator.Current)!;
    T IEnumerator<T>.Current => ((T) Enumerator.Current)!;
}