using System.Reflection;

namespace ThreadSafeDbContextNet.QueryProviders;

public class ThrowingMonitorProxy
{
    private readonly object _internalMonitor;

    public ThrowingMonitorProxy(object internalMonitor)
    {
        _internalMonitor = internalMonitor;
    }

    public void Erase()
    {
        var throwingMonitorField = _internalMonitor.GetType()
            .GetField("_isInCriticalSection", BindingFlags.NonPublic | BindingFlags.Instance);
        throwingMonitorField.SetValue(_internalMonitor, 0);
    }
}