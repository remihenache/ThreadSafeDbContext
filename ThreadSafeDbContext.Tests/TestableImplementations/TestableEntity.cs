namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class TestableEntity
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TestableEntityDependency> Dependencies { get; set; } = new();
}