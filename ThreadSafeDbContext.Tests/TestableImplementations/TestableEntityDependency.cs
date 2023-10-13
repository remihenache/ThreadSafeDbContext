namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations;

public class TestableEntityDependency
{
    public Int32 ID { get; set; }
    public Int32 TestableEntityID { get; set; }
    public String Name { get; set; } = String.Empty;
}