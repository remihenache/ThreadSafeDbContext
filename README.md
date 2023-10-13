# ThreadSafeDbContext

A thread safe Entity Framework DbContext implementation

# Install

To use these extensions, install the nuget package for your C# project and ensure that the appropriate namespaces are
referenced. Make sure that you have the necessary dependencies and target framework version set correctly.

```shell
dotnet add package ThreadSafeDbContext
```

# Usages

Simply make your application DbContext inherit from ThreadSafeDbContext and you are good to go.
The ThreadSafeDbContext class is a wrapper around the DbContext class and provides thread safe access to the DbContext
class.

```csharp
public class MyDbContext : ThreadSafeDbContext
{
    public MyDbContext(DbContextOptionsBuilder<MyDbContext> optionsBuilder) : base(optionsBuilder)
    {
    }
}

// or

public class MyDbContext : ThreadSafeDbContext
{
    public MyDbContext()
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("connection string");
        base.OnConfiguring(optionsBuilder);
    }
}
``` 

# Pros and cons

## Pros

- Thread safe access to the DbContext class
- Standard compliant for the DbContext class
- Use the DbSet and Queryable properties as you would normally do

## Cons

- The only one "non compliant part" is the constructor, that use DbContextOptionsBuilder instead of DbContextOptions.
- The thread safety comes at a cost of performance. The performance hit is not significant but it is there.
- The project is not an official implementation from Microsoft. It is a custom implementation, and may not be ready for
  production use.

# Performance benchmark

| Method               |     Mean |   Error |  StdDev |
|----------------------|---------:|--------:|--------:|
| Standard DbContext   | 280.6 us | 4.30 us | 4.41 us |
| ThreadSafe DbContext | 291.1 us | 4.48 us | 5.16 us |

# More about

You can find more details about the implementation
here: [A Threadsafe implementation of DbContext](https://medium.com/@rhenache/a-threadsafe-implementation-of-dbcontext-bbd9959cdc30)

Feel free to explore and leverage these extensions.

### Note: This code snippet is a standalone C# implementation and can be integrated into your project to extend the DbContext
