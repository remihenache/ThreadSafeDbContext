using System.Collections;

namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

public class ThreadSafeEnumerator : IEnumerator
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

public class ThreadSafeEnumerator<T> : ThreadSafeEnumerator, IEnumerator<T>, IAsyncEnumerator<T>
{
    public ThreadSafeEnumerator(IEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim)
        : base(enumerator, semaphoreSlim)
    {
    }

    public async ValueTask<Boolean> MoveNextAsync()
    {
        await this.SemaphoreSlim.WaitAsync();
        try
        {
            if (this.Enumerator is IAsyncEnumerator<T> asyncEnumerator)
                return await asyncEnumerator.MoveNextAsync();
            return (this.Enumerator as IEnumerator<T>)!.MoveNext();
        }
        finally
        {
            this.SemaphoreSlim.Release();
        }
    }

    T IAsyncEnumerator<T>.Current => ((T) this.Enumerator.Current)!;

    public async ValueTask DisposeAsync()
    {
        if (this.Enumerator is not IAsyncDisposable disposable)
            return;
        await this.SemaphoreSlim.WaitAsync();
        try
        {
            await disposable.DisposeAsync();
        }
        finally
        {
            this.SemaphoreSlim.Release();
        }
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