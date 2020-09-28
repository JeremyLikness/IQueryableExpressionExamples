using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    public class TestIterationResult
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Iteration { get; set; }

        public long DurationTicks { get; set; }

        [ForeignKey(nameof(TestResult))]
        public string TestResultId { get; set; }
    }
}
