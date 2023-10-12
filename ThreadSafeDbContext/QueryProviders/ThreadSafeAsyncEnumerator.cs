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

    public async ValueTask<Boolean> MoveNextAsync()
    {
        await this.semaphoreSlim.WaitAsync();
        try
        {
            return await this.enumerator.MoveNextAsync();
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }

    T IAsyncEnumerator<T>.Current => this.enumerator.Current;

    public async ValueTask DisposeAsync()
    {
        await this.semaphoreSlim.WaitAsync();
        try
        {
            await this.enumerator.DisposeAsync();
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}