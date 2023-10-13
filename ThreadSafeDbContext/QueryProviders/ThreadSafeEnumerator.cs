using System.Collections;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal class ThreadSafeEnumerator : IEnumerator
{
    protected readonly IEnumerator Enumerator;
    protected readonly SemaphoreSlim SemaphoreSlim;

    public ThreadSafeEnumerator(IEnumerator enumerator, SemaphoreSlim semaphoreSlim)
    {
        this.Enumerator = enumerator;
        this.SemaphoreSlim = semaphoreSlim;
    }

    public Boolean MoveNext()
    {
        this.SemaphoreSlim.Wait();
        try
        {
            return this.Enumerator.MoveNext();
        }
        finally
        {
            this.SemaphoreSlim.Release();
        }
    }

    public void Reset()
    {
        this.SemaphoreSlim.Wait();
        try
        {
            this.Enumerator.Reset();
        }
        finally
        {
            this.SemaphoreSlim.Release();
        }
    }

    public Object Current => this.Enumerator.Current!;
}

internal sealed class ThreadSafeEnumerator<T> : ThreadSafeEnumerator, IEnumerator<T>
{
    public ThreadSafeEnumerator(IEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim)
        : base(enumerator, semaphoreSlim)
    {
    }


    public void Dispose()
    {
        if (this.Enumerator is not IDisposable disposable) return;
        this.SemaphoreSlim.Wait();
        try
        {
            disposable.Dispose();
        }
        finally
        {
            this.SemaphoreSlim.Release();
        }
    }

    T IEnumerator<T>.Current => ((T) this.Enumerator.Current)!;
}