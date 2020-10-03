// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestDatabase
{
    /// <summary>
    /// Represents a group of tests at the assembly level.
    /// </summary>
    public class TestAssembly
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the assembly name.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the total duration of tests.
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Gets or sets the list of groups.
        /// </summary>
        public virtual ICollection<TestGroup> TestGroups { get; set; } =
            new List<TestGroup>();

        /// <summary>
        /// Adds a child <see cref="TestGroup"/>.
        /// </summary>
        /// <param name="group">The <see cref="TestGroup"/> to add.</param>
        public void AddGroup(TestGroup group)
        {
            group.TestAssemblyId = Id;
            TestGroups.Add(group);
            DurationTicks += group.DurationTicks;
        }
    }
}
