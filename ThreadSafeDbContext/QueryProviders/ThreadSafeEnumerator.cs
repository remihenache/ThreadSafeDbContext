using System.Collections;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal class ThreadSafeEnumerator : IEnumerator
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

    public object Current => Enumerator.Current!;
}

internal sealed class ThreadSafeEnumerator<T> : ThreadSafeEnumerator, IEnumerator<T>
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

    T IEnumerator<T>.Current => ((T)Enumerator.Current)!;
}