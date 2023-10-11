# ThreadSafeDbContext
A thread safe Entity Framework DbContext implementation

# Install
To use these extensions, install the nuget package for your C# project and ensure that the appropriate namespaces are referenced. Make sure that you have the necessary dependencies and target framework version set correctly.
```shell
dotnet add package ThreadSafeDbContext
```
# Usages
Simply make your application DbContext inherit from ThreadSafeDbContext and you are good to go. 
The ThreadSafeDbContext class is a wrapper around the DbContext class and provides thread safe access to the DbContext class. 
```csharp
public class MyDbContext : ThreadSafeDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
}
``` 


Feel free to explore and leverage these extensions.

### Note: This code snippet is a standalone C# implementation and can be integrated into your project to extend the DbContext
