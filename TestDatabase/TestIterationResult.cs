// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabase
{
    /// <summary>
    /// The result of an iteration of a test.
    /// </summary>
    public class TestIterationResult
    {
        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the descriptive text of the iteration.
        /// </summary>
        public string Iteration { get; set; }

        /// <summary>
        /// Gets or sets the duration of the iteration.
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Gets or sets the id of the related test.
        /// </summary>
        [ForeignKey(nameof(TestResult))]
        public string TestResultId { get; set; }
    }
}
