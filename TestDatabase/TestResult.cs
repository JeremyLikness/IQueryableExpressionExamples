// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    /// <summary>
    /// The result of a test.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Gets or sets the duration of the test run.
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Gets or sets the id of the group the test belongs to.
        /// </summary>
        [ForeignKey(nameof(TestGroup))]
        public string TestGroupId { get; set; }

        /// <summary>
        /// Gets or sets the list of iterations of the test.
        /// </summary>
        public virtual ICollection<TestIterationResult> IterationResults { get; set; }
            = new List<TestIterationResult>();

        /// <summary>
        /// Add an iteration to the test.
        /// </summary>
        /// <param name="iteration">The iteration to add.</param>
        public void AddIteration(TestIterationResult iteration)
        {
            iteration.TestResultId = Id;
            IterationResults.Add(iteration);
            DurationTicks += iteration.DurationTicks;
        }
    }
}
