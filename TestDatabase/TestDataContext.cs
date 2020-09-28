using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestDatabase
{
    public class TestDataContext : DbContext
    {
        public TestDataContext() { }
        
        public TestDataContext(DbContextOptions<TestDataContext> options)
            : base(options) { }

        public DbSet<TestAssembly> TestAssemblies { get; set; }
        public DbSet<TestGroup> TestGroups { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<TestIterationResult> TestIterationResults { get; set; }
    }
}
