using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSafeDbContextNet;

public class ThreadSafeDbContext : DbContext
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    public ThreadSafeDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
    {
        Configuration.ProxyCreationEnabled = false;
    }

    public ThreadSafeDbContext(string nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model)
    {
    }

    public ThreadSafeDbContext(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
    {
    }

    public ThreadSafeDbContext(
        DbConnection existingConnection,
        DbCompiledModel model,
        bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)
    {
    }

    public ThreadSafeDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext)
    {
    }


    public override DbSet<TEntity> Set<TEntity>()
    {
        _semaphoreSlim.Wait();
        try
        {
            return new ThreadSafeDbSet<TEntity>(base.Set<TEntity>(), _semaphoreSlim);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }


    public override async Task<int> SaveChangesAsync()
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            return await base.SaveChangesAsync();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public override int SaveChanges()
    {
        _semaphoreSlim.Wait();
        try
        {
            return base.SaveChanges();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public new void Dispose()
    {
        _semaphoreSlim.Dispose();
        base.Dispose();
    }
}