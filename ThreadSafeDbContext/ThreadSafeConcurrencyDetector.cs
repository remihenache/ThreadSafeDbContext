using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ThreadSafeDbContext;

internal class ThreadSafeConcurrencyDetector : IConcurrencyDetector
{
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConcurrencyDetectorCriticalSectionDisposer EnterCriticalSection()
    {
        return new ConcurrencyDetectorCriticalSectionDisposer(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ExitCriticalSection()
    {
    }
}