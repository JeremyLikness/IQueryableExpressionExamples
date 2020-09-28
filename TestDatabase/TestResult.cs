using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    public class TestResult
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string TestName { get; set; }

        public long DurationTicks { get; set; }

        [ForeignKey(nameof(TestGroup))]
        public string TestGroupId { get; set; }

        public virtual ICollection<TestIterationResult> IterationResults { get; set; }
            = new List<TestIterationResult>();

        public void AddIteration(TestIterationResult iteration)
        {
            iteration.TestResultId = Id;
            IterationResults.Add(iteration);
            DurationTicks += iteration.DurationTicks;
        }
    }
}
