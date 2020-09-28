using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    public class TestGroup
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string GroupName { get; set; }

        public long DurationTicks { get; set; }

        [ForeignKey(nameof(TestAssembly))]
        public string TestAssemblyId { get; set; }

        public virtual ICollection<TestResult> TestResults { get; set; }
            = new List<TestResult>();

        public void AddTestResult(TestResult result)
        {
            result.TestGroupId = Id;
            TestResults.Add(result);
            DurationTicks += result.DurationTicks;
        }
    }
}
