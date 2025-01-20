namespace Microsoft.EntityFrameworkCore.ThreadSafe.QueryProviders;

internal sealed class ThreadSafeAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IAsyncEnumerator<T> _enumerator;
    private readonly SemaphoreSlim _semaphoreSlim;

    public ThreadSafeAsyncEnumerator(IAsyncEnumerator<T> enumerator, SemaphoreSlim semaphoreSlim)
    {
        _enumerator = enumerator;
        _semaphoreSlim = semaphoreSlim;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            return await _enumerator.MoveNextAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    T IAsyncEnumerator<T>.Current => _enumerator.Current;

    public async ValueTask DisposeAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            await _enumerator.DisposeAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}