using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestDatabase
{
    public class TestAssembly
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string AssemblyName { get; set; }

        public long DurationTicks { get; set; }

        public virtual ICollection<TestGroup> TestGroups { get; set; } =
            new List<TestGroup>();

        public void AddGroup(TestGroup group)
        {
            group.TestAssemblyId = Id;
            TestGroups.Add(group);
            DurationTicks += group.DurationTicks;
        }
    }
}
