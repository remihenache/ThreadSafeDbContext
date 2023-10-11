using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using ThreadSafeDbContext.QueryProviders;

namespace ThreadSafeDbContext;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal sealed class ThreadSafeDbSet<TEntity> :
    InternalDbSet<TEntity>,
    IQueryable<TEntity>,
    IAsyncEnumerable<TEntity>
    where TEntity : class
{
    private readonly SemaphoreSlim semaphoreSlim;
    private readonly DbSet<TEntity> set;

    public ThreadSafeDbSet(DbContext context, DbSet<TEntity> set, SemaphoreSlim semaphoreSlim)
        : base(context, set.EntityType.Name)
    {
        this.set = set;
        this.semaphoreSlim = semaphoreSlim;
    }

    public override IEntityType EntityType => this.set.EntityType;

    public override IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        try
        {
            this.semaphoreSlim.Wait(cancellationToken);
            return (this.set as IAsyncEnumerable<TEntity>)!.GetAsyncEnumerator(cancellationToken);
        }
        finally
        {
            if (this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }

    public IQueryProvider Provider =>
        new ThreadSafeQueryProvider((this.set as IQueryable).Provider, this.semaphoreSlim);


    public IEnumerator<TEntity> GetEnumerator()
    {
        try
        {
            this.semaphoreSlim.Wait();
            return (this.set as IQueryable<TEntity>)!.GetEnumerator();
        }
        finally
        {
            if (this.semaphoreSlim.CurrentCount == 0)
                this.semaphoreSlim.Release();
        }
    }

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        this.semaphoreSlim.Wait();
        EntityEntry<TEntity> result = base.Add(entity);
        this.semaphoreSlim.Release();
        return result;
    }

    public override EntityEntry<TEntity> Attach(TEntity entity)
    {
        this.semaphoreSlim.Wait();
        EntityEntry<TEntity> result = base.Attach(entity);
        this.semaphoreSlim.Release();
        return result;
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        this.semaphoreSlim.Wait();
        EntityEntry<TEntity> result = base.Update(entity);
        this.semaphoreSlim.Release();
        return result;
    }

    public override async ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = new())
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);
        EntityEntry<TEntity> result = await base.AddAsync(entity);
        this.semaphoreSlim.Release();
        return result;
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        this.semaphoreSlim.Wait();
        EntityEntry<TEntity> result = base.Remove(entity);
        this.semaphoreSlim.Release();
        return result;
    }

    public override void AddRange(params TEntity[] entities)
    {
        this.semaphoreSlim.Wait();
        base.AddRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        this.semaphoreSlim.Wait();
        base.AddRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void AttachRange(params TEntity[] entities)
    {
        this.semaphoreSlim.Wait();
        base.AttachRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void AttachRange(IEnumerable<TEntity> entities)
    {
        this.semaphoreSlim.Wait();
        base.AttachRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        this.semaphoreSlim.Wait();
        base.RemoveRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        this.semaphoreSlim.Wait();
        base.RemoveRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        this.semaphoreSlim.Wait();
        base.UpdateRange(entities);
        this.semaphoreSlim.Release();
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        this.semaphoreSlim.Wait();
        base.UpdateRange(entities);
        this.semaphoreSlim.Release();
    }

    public override async Task AddRangeAsync(params TEntity[] entities)
    {
        await this.semaphoreSlim.WaitAsync();
        await base.AddRangeAsync(entities);
        this.semaphoreSlim.Release();
    }

    public override async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new())
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);
        await base.AddRangeAsync(entities);
        this.semaphoreSlim.Release();
    }
}