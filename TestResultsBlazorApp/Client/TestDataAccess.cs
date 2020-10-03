// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressionPowerTools.Serialization.EFCore.Http.Extensions;
using ExpressionPowerTools.Serialization.EFCore.Http.Queryable;
using Microsoft.EntityFrameworkCore;
using TestDatabase;
using TestResultsBlazorApp.Shared;

namespace TestResultsBlazorApp.Client
{
    /// <summary>
    /// Handles the queries for the tests.
    /// </summary>
    public class TestDataAccess
    {
        /// <summary>
        /// Milliseconds.
        /// </summary>
        private readonly TimeSpan ms = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Seconds.
        /// </summary>
        private readonly TimeSpan sec = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The dictionary that holds times for statistics.
        /// </summary>
        private readonly IDictionary<string, IDictionary<string, TimeSpan>> times =
            new Dictionary<string, IDictionary<string, TimeSpan>>
            {
                {
                    nameof(Fastest), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0) },
                    }
                },
                {
                    nameof(Slowest), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(1) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(1) },
                        { nameof(TestResult), TimeSpan.FromSeconds(1) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(1) },
                    }
                },
                {
                    nameof(Median), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0.5) },
                    }
                },
                {
                    nameof(Mean), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0.5) },
                    }
                },
            };

        /// <summary>
        /// Gets or sets the event raised when new data is loaded.
        /// </summary>
        public EventHandler LoadAsyncCalled { get; set; }

        /// <summary>
        /// Gets the list of test entries.
        /// </summary>
        public List<TestEntry> TestEntries { get; private set; } =
            new List<TestEntry>();

        /// <summary>
        /// Gets the filter for assemblies.
        /// </summary>
        public IEnumerable<TestEntry> Assemblies
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestAssembly));
        }

        /// <summary>
        /// Gets the filter for groups.
        /// </summary>
        public IEnumerable<TestEntry> Groups
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestGroup));
        }

        /// <summary>
        /// Gets the filter for tests.
        /// </summary>
        public IEnumerable<TestEntry> TestResults
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestResult));
        }

        /// <summary>
        /// Gets the filter for iterations.
        /// </summary>
        public IEnumerable<TestEntry> Iterations
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestIterationResult));
        }

        /// <summary>
        /// Gets the most specific type in the hierarchy that is
        /// currently loaded.
        /// </summary>
        public string MostSpecificType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AssemblyId))
                {
                    return nameof(TestAssembly);
                }

                if (string.IsNullOrWhiteSpace(GroupId))
                {
                    return nameof(TestGroup);
                }

                if (string.IsNullOrWhiteSpace(TestResultId) ||
                    !Iterations.Any())
                {
                    return nameof(TestResult);
                }

                return nameof(TestIterationResult);
            }
        }

        /// <summary>
        /// Gets a value indicating whether data is loading.
        /// </summary>
        public bool Loading { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the sort is on the
        /// name property, otherwise the duration property.
        /// </summary>
        public bool SortName { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether the sort is ascending or
        /// descending.
        /// </summary>
        public bool SortAscending { get; private set; } = false;

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        public string Filter { get; set; } = string.Empty;

        /// <summary>
        /// Gets the error message from the last operation.
        /// </summary>
        public string ErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the id of the currently selected assembly.
        /// </summary>
        public string AssemblyId { get; private set; } = null;

        /// <summary>
        /// Gets the name of the curently selected assembly.
        /// </summary>
        public string AssemblyName { get; private set; } = null;

        /// <summary>
        /// Gets the id of the currently selected group.
        /// </summary>
        public string GroupId { get; private set; } = null;

        /// <summary>
        /// Gets the name of the currently selected group.
        /// </summary>
        public string GroupName { get; private set; } = null;

        /// <summary>
        /// Gets the id of the currently selected test result.
        /// </summary>
        public string TestResultId { get; private set; } = null;

        /// <summary>
        /// Gets the name of the currently selected test result.
        /// </summary>
        public string TestName { get; private set; } = null;

        /// <summary>
        /// Gets the fastest time for the type.
        /// </summary>
        /// <param name="type">The type of test grouping.</param>
        /// <returns>The fastest time.</returns>
        public TimeSpan Fastest(string type) => times[nameof(Fastest)][type];

        /// <summary>
        /// Gets the slowest time for the type.
        /// </summary>
        /// <param name="type">The type of test grouping.</param>
        /// <returns>The slowest time.</returns>
        public TimeSpan Slowest(string type) => times[nameof(Slowest)][type];

        /// <summary>
        /// Gets the median time for the type.
        /// </summary>
        /// <param name="type">The type of test grouping.</param>
        /// <returns>The median time.</returns>
        public TimeSpan Median(string type) => times[nameof(Median)][type];

        /// <summary>
        /// Gets the average time for the type.
        /// </summary>
        /// <param name="type">The type of test grouping.</param>
        /// <returns>The aveage time.</returns>
        public TimeSpan Mean(string type) => times[nameof(Mean)][type];

        /// <summary>
        /// Convert ticks into a friendly display.
        /// </summary>
        /// <param name="durationTicks">The duration ticks.</param>
        /// <returns>The friendly display.</returns>
        public string DurationDisplay(long durationTicks)
            => DurationDisplay(TimeSpan.FromTicks(durationTicks));

        /// <summary>
        /// Provides a friendly display for spanned time.
        /// </summary>
        /// <param name="duration">The <see cref="TimeSpan"/>.</param>
        /// <returns>The time round to nanonseconds or milliseconds.</returns>
        public string DurationDisplay(TimeSpan duration)
        {
            if (duration < ms)
            {
                return $"{Math.Floor(duration.TotalMilliseconds * 1000)} ns";
            }

            if (duration < sec)
            {
                return $"{Math.Floor(duration.TotalMilliseconds)} ms";
            }

            var roundedSeconds = Math.Floor(duration.TotalSeconds * 10) / 10;
            return $"{roundedSeconds} s";
        }

        /// <summary>
        /// Set the current test id.
        /// </summary>
        /// <param name="testId">The id of the test.</param>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        public Task SetTest(string testId)
        {
            if (testId == TestResultId)
            {
                TestResultId = string.Empty;
            }
            else
            {
                TestResultId = testId;
            }

            TestName = TestEntries.Where(
                    t => t.Type == nameof(TestResult) &&
                    t.Id == TestResultId)
                .Select(t => t.Name)
                    .FirstOrDefault();

            return LoadAsync();
        }

        /// <summary>
        /// Sets the currently selected group.
        /// </summary>
        /// <param name="groupId">The id of the <see cref="TestGroup"/>.</param>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        public Task SetGroup(string groupId)
        {
            if (groupId == GroupId)
            {
                GroupId = string.Empty;
            }
            else
            {
                GroupId = groupId;
            }

            GroupName = TestEntries.Where(
                    t => t.Type == nameof(TestGroup) &&
                    t.Id == GroupId)
                .Select(t => t.Name)
                    .FirstOrDefault();

            TestResultId = TestName = string.Empty;
            return LoadAsync();
        }

        /// <summary>
        /// Sets the currently selected assembly.
        /// </summary>
        /// <param name="assemblyId">The id of the <see cref="TestAssembly"/>.</param>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        public Task SetAssembly(string assemblyId)
        {
            if (assemblyId == AssemblyId)
            {
                AssemblyId = string.Empty;
            }
            else
            {
                AssemblyId = assemblyId;
            }

            AssemblyName = TestEntries.Where(
                    t => t.Type == nameof(TestAssembly) &&
                    t.Id == AssemblyId)
                .Select(t => t.Name)
                    .FirstOrDefault();

            GroupId = GroupName = string.Empty;
            TestResultId = TestName = string.Empty;
            return LoadAsync();
        }

        /// <summary>
        /// Modifies the sort.
        /// </summary>
        /// <param name="sortName">A flag indicating whether it should sort by name.</param>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        /// <remarks>This toggle works for both sort column and direction. If
        /// the value is passed for the same column, the sort direction is toggled.</remarks>
        public Task SortNameAsync(bool sortName)
        {
            if (SortName == sortName)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                SortName = sortName;
            }

            return LoadAsync();
        }

        /// <summary>
        /// Load data based on the current sorts and filters.
        /// </summary>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        public async Task LoadAsync()
        {
            if (Loading)
            {
                return;
            }

            Loading = true;
            TestEntries = null;
            ErrorMessage = string.Empty;

            var useFilter = false;
            string search = string.Empty;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                search = Filter.Trim();
                if (search.Length > 2)
                {
                    useFilter = true;
                }
            }

            var query = DbClientContext<TestDataContext>.Query(
                ctx => ctx.TestAssemblies);

            if (useFilter)
            {
                if (string.IsNullOrEmpty(AssemblyId))
                {
                    query = query.Where(a =>
                        EF.Functions.Like(a.AssemblyName, $"{search}%") ||
                        EF.Functions.Like(a.AssemblyName, $"%{search}%") ||
                        EF.Functions.Like(a.AssemblyName, $"%{search}"));
                }
                else
                {
                    query = query.Where(a => a.Id == AssemblyId ||
                        EF.Functions.Like(a.AssemblyName, $"{search}%") ||
                        EF.Functions.Like(a.AssemblyName, $"%{search}%") ||
                        EF.Functions.Like(a.AssemblyName, $"%{search}"));
                }
            }

            if (SortName)
            {
                query = SortAscending ?
                    query.OrderBy(t => t.AssemblyName) :
                    query.OrderByDescending(t => t.AssemblyName);
            }
            else
            {
                query = SortAscending ?
                    query.OrderBy(t => t.DurationTicks) :
                    query.OrderByDescending(t => t.DurationTicks);
            }

            var testEntries = query.Select(
                    a => new TestEntry(
                    a.Id,
                    nameof(TestAssembly),
                    a.AssemblyName,
                    a.DurationTicks,
                    string.Empty));
            try
            {
                var assemblyEntries = await testEntries.ExecuteRemote().ToListAsync();
                TestEntries = new List<TestEntry>(assemblyEntries);
                ComputeTimesFor(nameof(TestAssembly));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            if (!string.IsNullOrWhiteSpace(AssemblyId) &&
                string.IsNullOrWhiteSpace(ErrorMessage))
            {
                await LoadGroups(useFilter, search);
            }

            if (!string.IsNullOrWhiteSpace(GroupId) &&
                string.IsNullOrWhiteSpace(ErrorMessage))
            {
                await LoadTests(useFilter, search);
            }

            if (!string.IsNullOrWhiteSpace(TestResultId) &&
                string.IsNullOrWhiteSpace(ErrorMessage))
            {
                await LoadIterations(useFilter, search);
            }

            Loading = false;
            LoadAsyncCalled?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Computes time statistics for a grouping.
        /// </summary>
        /// <param name="type">The type of grouping.</param>
        private void ComputeTimesFor(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return;
            }

            var count = TestEntries.Count(te => te.Type == type);

            if (count == 0)
            {
                times[nameof(Fastest)][type] = TimeSpan.FromSeconds(0);
                times[nameof(Slowest)][type] = TimeSpan.FromSeconds(1);
                times[nameof(Median)][type] = TimeSpan.FromSeconds(0.5);
                times[nameof(Mean)][type] = TimeSpan.FromSeconds(0.5);
                return;
            }

            times[nameof(Fastest)][type] = TimeSpan.FromTicks(
                TestEntries.Where(te => te.Type == type).Min(te => te.DurationTicks));

            times[nameof(Slowest)][type] = TimeSpan.FromTicks(
                TestEntries.Where(te => te.Type == type).Max(te => te.DurationTicks));

            var orderBy = TestEntries.Where(te => te.Type == type)
                .OrderBy(te => te.DurationTicks);

            var middle = count / 2;
            times[nameof(Median)][type] = TimeSpan.FromTicks(orderBy.ElementAt(middle).DurationTicks);

            times[nameof(Mean)][type] = TimeSpan.FromTicks(
                (long)TestEntries.Where(te => te.Type == type).Average(te => te.DurationTicks));
        }

        private async Task LoadIterations(bool useFilter, string search)
        {
            var iterationQuery = DbClientContext<TestDataContext>.Query(
                    ctx => ctx.TestIterationResults)
                    .Where(t => t.TestResultId == TestResultId);

            if (useFilter)
            {
                iterationQuery = iterationQuery.Where(i =>
                    EF.Functions.Like(i.Iteration, $"{search}%") ||
                    EF.Functions.Like(i.Iteration, $"%{search}%") ||
                    EF.Functions.Like(i.Iteration, $"%{search}"));
            }

            switch (SortName)
            {
                case true:
                    iterationQuery = SortAscending ?
                        iterationQuery.OrderBy(i => i.Iteration) :
                        iterationQuery.OrderByDescending(i => i.Iteration);
                    break;
                case false:
                    iterationQuery = SortAscending ?
                        iterationQuery.OrderBy(t => t.DurationTicks) :
                        iterationQuery.OrderByDescending(t => t.DurationTicks);
                    break;
            }

            var testEntries = iterationQuery.Select(
                i => new TestEntry(
                    i.Id,
                    nameof(TestIterationResult),
                    i.Iteration,
                    i.DurationTicks,
                    i.TestResultId));

            try
            {
                var iterationResults = await testEntries.ExecuteRemote().ToListAsync();
                TestEntries.AddRange(iterationResults);
                ComputeTimesFor(nameof(TestIterationResult));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task LoadTests(bool useFilter, string search)
        {
            var testQuery = DbClientContext<TestDataContext>.Query(
                    ctx => ctx.TestResults)
                    .Where(t => t.TestGroupId == GroupId);

            if (useFilter)
            {
                if (string.IsNullOrWhiteSpace(TestResultId))
                {
                    testQuery = testQuery.Where(t =>
                        EF.Functions.Like(t.TestName, $"{search}%") ||
                        EF.Functions.Like(t.TestName, $"%{search}%") ||
                        EF.Functions.Like(t.TestName, $"%{search}"));
                }
                else
                {
                    testQuery = testQuery.Where(t => t.Id == TestResultId ||
                        EF.Functions.Like(t.TestName, $"{search}%") ||
                        EF.Functions.Like(t.TestName, $"%{search}%") ||
                        EF.Functions.Like(t.TestName, $"%{search}"));
                }
            }

            switch (SortName)
            {
                case true:
                    testQuery = SortAscending ?
                        testQuery.OrderBy(t => t.TestName) :
                        testQuery.OrderByDescending(t => t.TestName);
                    break;
                case false:
                    testQuery = SortAscending ?
                        testQuery.OrderBy(t => t.DurationTicks) :
                        testQuery.OrderByDescending(t => t.DurationTicks);
                    break;
            }

            var testEntries = testQuery.Select(
                t => new TestEntry(
                    t.Id,
                    nameof(TestResult),
                    t.TestName,
                    t.DurationTicks,
                    t.TestGroupId));
            try
            {
                var testResults = await testEntries.ExecuteRemote().ToListAsync();
                TestEntries.AddRange(testResults);
                ComputeTimesFor(nameof(TestResult));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private async Task LoadGroups(bool useFilter, string search)
        {
            var groupQuery = DbClientContext<TestDataContext>.Query(
                ctx => ctx.TestGroups)
                .Where(g => g.TestAssemblyId == AssemblyId);

            if (useFilter)
            {
                if (string.IsNullOrEmpty(GroupId))
                {
                    groupQuery = groupQuery.Where(g =>
                        EF.Functions.Like(g.GroupName, $"{search}%") ||
                        EF.Functions.Like(g.GroupName, $"%{search}%") ||
                        EF.Functions.Like(g.GroupName, $"%{search}"));
                }
                else
                {
                    groupQuery = groupQuery.Where(g => g.Id == GroupId ||
                        EF.Functions.Like(g.GroupName, $"{search}%") ||
                        EF.Functions.Like(g.GroupName, $"%{search}%") ||
                        EF.Functions.Like(g.GroupName, $"%{search}"));
                }
            }

            switch (SortName)
            {
                case true:
                    groupQuery = SortAscending ?
                        groupQuery.OrderBy(t => t.GroupName) :
                        groupQuery.OrderByDescending(t => t.GroupName);
                    break;
                case false:
                    groupQuery = SortAscending ?
                        groupQuery.OrderBy(t => t.DurationTicks) :
                        groupQuery.OrderByDescending(t => t.DurationTicks);
                    break;
            }

            var testEntries = groupQuery.Select(
                g => new TestEntry(
                    g.Id,
                    nameof(TestGroup),
                    g.GroupName,
                    g.DurationTicks,
                    g.TestAssemblyId));

            try
            {
                var groupResults = await testEntries.ExecuteRemote().ToListAsync();
                TestEntries.AddRange(groupResults);
                ComputeTimesFor(nameof(TestGroup));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
