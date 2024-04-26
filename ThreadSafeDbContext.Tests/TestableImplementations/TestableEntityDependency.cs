namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class TestableEntityDependency
{
    public int ID { get; set; }
    public int TestableEntityID { get; set; }
    public string Name { get; set; } = string.Empty;
}