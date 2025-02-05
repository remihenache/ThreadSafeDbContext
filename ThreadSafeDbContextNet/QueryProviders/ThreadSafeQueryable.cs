using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;

namespace Microsoft.EntityFrameworkNet.ThreadSafe.QueryProviders;

internal class ThreadSafeQueryable : IOrderedQueryable, IDbAsyncEnumerable
{
    protected readonly SemaphoreSlim SemaphoreSlim;
    protected readonly IQueryable Set;

    public ThreadSafeQueryable(IQueryable set, SemaphoreSlim semaphoreSlim)
    {
        Set = set;
        SemaphoreSlim = semaphoreSlim;
    }


    public IEnumerator GetEnumerator()
    {
        return new ThreadSafeEnumerator(Set.GetEnumerator(), SemaphoreSlim);
    }

    public Type ElementType => Set.ElementType;

    public Expression Expression => Set.Expression;

    public IQueryProvider Provider => new ThreadSafeQueryProvider(Set.Provider, SemaphoreSlim);


    public IDbAsyncEnumerator GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator(Set.GetEnumerator(), SemaphoreSlim);
    }
    
    
    
}

internal class ThreadSafeQueryable<T> : ThreadSafeQueryable, IOrderedQueryable<T>, IDbAsyncEnumerable<T>
{
    public ThreadSafeQueryable(IQueryable<T> set, SemaphoreSlim semaphoreSlim)
        : base(set, semaphoreSlim)
    {
        DisableThrowingMonitor();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public new IEnumerator<T> GetEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim);
    }

    public IDbAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new ThreadSafeEnumerator<T>((Set as IQueryable<T>)!.GetEnumerator(), SemaphoreSlim);
    }

    IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
    {
        return GetAsyncEnumerator();
    }
    
    
    private void DisableThrowingMonitor()
    {
        // Accéder à _internalQuery
        var internalQueryField = Set.GetType()
            .GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance);
        if (internalQueryField == null)
        {
            throw new InvalidOperationException("Field '_internalQuery' not found in DbSet.");
        }

        var internalQuery = internalQueryField.GetValue(Set);
        if (internalQuery == null)
        {
            throw new InvalidOperationException("InternalQuery is null.");
        }

        // Accéder à ObjectQuery
        var objectQueryProperty = internalQuery.GetType()
            .GetProperty("ObjectQuery");
        if (objectQueryProperty == null)
        {
            throw new InvalidOperationException("Property 'ObjectQuery' not found in InternalQuery.");
        }

        var objectQuery = objectQueryProperty.GetValue(internalQuery) as ObjectQuery;
        if (objectQuery == null)
        {
            throw new InvalidOperationException("ObjectQuery is null.");
        }
        //
        // // Désactiver ThrowingMonitor
        // var objectQueryProviderProperty = objectQuery.GetType()
        //     .GetProperty("ObjectQueryProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        //
        // if (objectQueryProviderProperty == null)
        // {
        //     throw new InvalidOperationException("Field '_throwingMonitor' not found in ObjectQuery.");
        // }
        // var objectQueryProvider = objectQueryProviderProperty.GetValue(objectQuery);
        // if (objectQueryProvider == null)
        // {
        //     throw new InvalidOperationException("ObjectQueryProvider is null.");
        // }
        var contextField = objectQuery.GetType()
            .GetProperty("Context");
        if (contextField == null)
        {
            throw new InvalidOperationException("_context is null.");
        }
        var context = contextField.GetValue(objectQuery);
        if (context == null)
        {
            throw new InvalidOperationException("Context is null.");
        }
        var throwingMonitorField = context.GetType()
            .GetField("_asyncMonitor", BindingFlags.NonPublic | BindingFlags.Instance);
        //var fakeMonitor = MonitorHack.CreateFakeThrowingMonitor();
        var originalMonitor = throwingMonitorField.GetValue(context);

        if (originalMonitor == null)
        {
            throw new InvalidOperationException("Current '_asyncMonitor' instance is null.");
        }

        // Créer un proxy autour de l'instance existante
        var proxyMonitor = ThrowingMonitorProxy<object>.Create(originalMonitor);

        // Remplacer le champ _asyncMonitor par le proxy
        throwingMonitorField.SetValue(context, proxyMonitor);

    }
}


public class ThrowingMonitorProxy<T> : RealProxy
{
    private readonly T _instance;

    private ThrowingMonitorProxy(T instance)
        : base(typeof(T))
    {
        _instance = instance;
    }

    public static T Create(T instance)
    {
        return (T)new ThrowingMonitorProxy<T>(instance).GetTransparentProxy();
    }

    public override IMessage Invoke(IMessage msg)
    {
        var methodCall = (IMethodCallMessage)msg;
        var method = (MethodInfo)methodCall.MethodBase;

        try
        {
            Console.WriteLine("Intercepting method: " + method.Name);

            // Personnalisation des méthodes interceptees
            if (method.Name == "Enter")
            {
                Console.WriteLine("Custom behavior on Enter");
                return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            else if (method.Name == "Exit")
            {
                Console.WriteLine("Custom behavior on Exit");
                return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            else if (method.Name == "EnsureNotEntered")
            {
                Console.WriteLine("Custom behavior on EnsureNotEntered");
                return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);
            }

            // Appeler directement la méthode réelle de l'instance encapsulée dans les cas par défaut
            var result = method.Invoke(_instance, methodCall.InArgs);
            return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);

            // Intercepter et retourner une exception s'il y en a une
            if (e is TargetInvocationException && e.InnerException != null)
            {
                return new ReturnMessage(e.InnerException, methodCall);
            }

            return new ReturnMessage(e, methodCall);
        }
    }
}

