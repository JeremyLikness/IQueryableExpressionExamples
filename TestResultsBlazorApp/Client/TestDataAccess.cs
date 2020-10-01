using ExpressionPowerTools.Serialization.EFCore.Http.Extensions;
using ExpressionPowerTools.Serialization.EFCore.Http.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestDatabase;
using TestResultsBlazorApp.Shared;
using Microsoft.EntityFrameworkCore;

namespace TestResultsBlazorApp.Client
{
    public class TestDataAccess
    {
        public List<TestEntry> TestEntries { get; private set; } =
            new List<TestEntry>();

        public IEnumerable<TestEntry> Assemblies
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestAssembly));
        }

        public IEnumerable<TestEntry> Groups
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestGroup));
        }

        public IEnumerable<TestEntry> TestResults
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestResult));
        }

        public IEnumerable<TestEntry> Iterations
        {
            get => (TestEntries ?? new List<TestEntry>())
                .Where(t => t.Type == nameof(TestIterationResult));
        }

        private IDictionary<string, IDictionary<string, TimeSpan>> times =
            new Dictionary<string, IDictionary<string, TimeSpan>>
            {
                { nameof(Fastest), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0) }
                    }
                }
                ,
                { nameof(Slowest), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(1) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(1) },
                        { nameof(TestResult), TimeSpan.FromSeconds(1) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(1) }
                    }
                },
                { nameof(Median), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0.5) }
                    }
                },
                { nameof(Mean), new Dictionary<string, TimeSpan>
                    {
                        { nameof(TestAssembly), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestGroup), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestResult), TimeSpan.FromSeconds(0.5) },
                        { nameof(TestIterationResult), TimeSpan.FromSeconds(0.5) }
                    }
                },
            };

        public TimeSpan Fastest(string type) => times[nameof(Fastest)][type];

        public TimeSpan Slowest(string type) => times[nameof(Slowest)][type];

        public TimeSpan Median(string type) => times[nameof(Median)][type];

        public TimeSpan Mean(string type) => times[nameof(Mean)][type];

        protected void ComputeTimesFor(string type)
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

        public bool Loading { get; private set; } = false;
        public bool SortName { get; private set; } = false;
        public bool SortAscending { get; private set; } = false;
        public string Filter { get; set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public string AssemblyId { get; private set; } = null;
        public string AssemblyName { get; private set; } = null;
        public string GroupId { get; private set; } = null;
        public string GroupName { get; private set; } = null;
        public string TestResultId { get; private set; } = null;
        public string TestName { get; private set; } = null;

        public EventHandler LoadAsyncCalled;

        private readonly TimeSpan Ms = TimeSpan.FromMilliseconds(1);
        private readonly TimeSpan Sec = TimeSpan.FromSeconds(1);

        public string DurationDisplay(long durationTicks)
            => DurationDisplay(TimeSpan.FromTicks(durationTicks));

        public string DurationDisplay(TimeSpan duration)
        {
            if (duration < Ms)
            {
                return $"{Math.Floor(duration.TotalMilliseconds * 1000)} ns";
            }

            if (duration < Sec)
            {
                return $"{Math.Floor(duration.TotalMilliseconds)} ms";
            }

            var roundedSeconds = Math.Floor(duration.TotalSeconds * 10) / 10;
            return $"{roundedSeconds} s";
        }

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
