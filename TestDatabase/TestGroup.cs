// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    /// <summary>
    /// Represents a group of tests.
    /// </summary>
    public class TestGroup
    {
        /// <summary>
        /// Gets or sets the id of the group.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the duration of tests in the group.
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Gets or sets the assembly id.
        /// </summary>
        [ForeignKey(nameof(TestAssembly))]
        public string TestAssemblyId { get; set; }

        /// <summary>
        /// Gets or sets the collection of test results in the group.
        /// </summary>
        public virtual ICollection<TestResult> TestResults { get; set; }
            = new List<TestResult>();

        /// <summary>
        /// Add a new test result to the group.
        /// </summary>
        /// <param name="result">The <see cref="TestResult"/> to add.</param>
        public void AddTestResult(TestResult result)
        {
            result.TestGroupId = Id;
            TestResults.Add(result);
            DurationTicks += result.DurationTicks;
        }
    }
}
