namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal sealed class ThreadSafeAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IAsyncEnumerator<T> enumerator;
    private readonly SemaphoreSlim semaphoreSlim;

    public ThreadSafeAsyncEnumerator(IAsyncEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim)
    {
        this.enumerator = enumerator;
        this.semaphoreSlim = semaphoreSlim;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            return await enumerator.MoveNextAsync();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    T IAsyncEnumerator<T>.Current => enumerator.Current;

    public async ValueTask DisposeAsync()
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            await enumerator.DisposeAsync();
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}