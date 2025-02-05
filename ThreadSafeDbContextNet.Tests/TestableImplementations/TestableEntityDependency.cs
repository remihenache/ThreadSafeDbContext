namespace Microsoft.EntityFrameworkCore.ThreadSafe.Tests.TestableImplementations
{
    public class TestableEntityDependency
    {
        public int Id { get; set; }
        public int TestableEntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public virtual TestableEntity TestableEntity { get; set; }
    }
}