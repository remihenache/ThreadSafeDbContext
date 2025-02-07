using System.Collections.Generic;

namespace ThreadSafeDbContextNet.Tests.TestableImplementations
{
    public class TestableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<TestableEntityDependency> Dependencies { get; set; }
    }
}