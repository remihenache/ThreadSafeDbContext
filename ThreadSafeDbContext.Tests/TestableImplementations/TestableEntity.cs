namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class TestableEntity
{
    public Int32 ID { get; set; }
    public String Name { get; set; } = String.Empty;
    public List<TestableEntityDependency> Dependencies { get; set; } = new();
}