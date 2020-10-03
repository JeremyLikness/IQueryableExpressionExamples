// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using Microsoft.EntityFrameworkCore;

namespace TestDatabase
{
    /// <summary>
    /// Data access for the test results database.
    /// </summary>
    public class TestDataContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataContext"/> class.
        /// </summary>
        public TestDataContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataContext"/> class
        /// with a set of <see cref="DbContextOptions{TContext}"/>.
        /// </summary>
        /// <param name="options">The options for configuration.</param>
        public TestDataContext(DbContextOptions<TestDataContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the set of assemblies.
        /// </summary>
        public DbSet<TestAssembly> TestAssemblies { get; set; }

        /// <summary>
        /// Gets or sets he groups of tests.
        /// </summary>
        public DbSet<TestGroup> TestGroups { get; set; }

        /// <summary>
        /// Gets or sets the individual test results.
        /// </summary>
        public DbSet<TestResult> TestResults { get; set; }

        /// <summary>
        /// Gets or sets the results per iteration of a tet.
        /// </summary>
        public DbSet<TestIterationResult> TestIterationResults { get; set; }
    }
}
