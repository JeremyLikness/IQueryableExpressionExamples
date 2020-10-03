// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace TestDatabase.SampleData
{
    /// <summary>
    /// Class to load sample data.
    /// </summary>
    public class SampleDataLoader
    {
        /// <summary>
        /// Check to see if the database exists. If not, create it and load
        /// the test data.
        /// </summary>
        /// <param name="dataContext">The <see cref="TestDataContext"/>/.</param>
        public void CheckAndSeed(TestDataContext dataContext)
        {
            if (dataContext.Database.EnsureCreated())
            {
                var data = GenerateSeed();
                dataContext.TestAssemblies.AddRange(data);
                dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// This generates the seeded database using the graph hierarchy
        /// starting with <see cref="TestAssembly"/> then
        /// <see cref="TestGroup"/>, <see cref="TestResult"/>, and finally
        /// <see cref="TestIterationResult"/>.
        /// </summary>
        /// <returns>The root assemblies.</returns>
        public ICollection<TestAssembly> GenerateSeed()
        {
            var assembly = GetType().Assembly;
            var graphRoot = new List<TestAssembly>();
            var resourceRoot = $"{GetType().Namespace}.";
            foreach (var file in new[] { "core.xml", "serialization.xml" })
            {
                var resource = $"{resourceRoot}{file}";
                var xml = string.Empty;
                using (var stream = assembly.GetManifestResourceStream(resource))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        xml = reader.ReadToEnd();
                    }
                }

                ProcessXml(graphRoot, xml);
            }

            return graphRoot;
        }

        /// <summary>
        /// Process the loaded XML for a test assembly.
        /// </summary>
        /// <param name="graphRoot">The root assemblies.</param>
        /// <param name="xml">The loaded test results XML.</param>
        private void ProcessXml(List<TestAssembly> graphRoot, string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var tests = doc.DocumentElement.ChildNodes[2];
            foreach (XmlNode child in tests.ChildNodes)
            {
                if (child.Name == "UnitTestResult")
                {
                    var testName = child.Attributes.GetNamedItem("testName").InnerText;
                    var duration = TimeSpan.Parse(child.Attributes.GetNamedItem("duration").InnerText);
                    var parts = TestNameParser(testName);
                    var assembly = parts[0];
                    parts.RemoveAt(0);
                    var group = parts[0];
                    string iteration = null;
                    parts.RemoveAt(0);
                    if (parts[parts.Count - 1].StartsWith("("))
                    {
                        iteration = parts[parts.Count - 1];
                        parts.RemoveAt(parts.Count - 1);
                    }

                    var test = string.Join(" ", parts);

                    TestIterationResult iterationResult = null;
                    if (!string.IsNullOrWhiteSpace(iteration))
                    {
                        iterationResult = new TestIterationResult
                        {
                            Iteration = iteration,
                            DurationTicks = duration.Ticks,
                        };
                    }

                    // test?
                    var testResult = graphRoot.SelectMany(r => r.TestGroups)
                        .SelectMany(g => g.TestResults)
                        .SingleOrDefault(t => t.TestName == test);

                    if (testResult == null)
                    {
                        testResult = new TestResult
                        {
                            TestName = test,
                            DurationTicks = iterationResult == null ?
                                duration.Ticks : 0,
                        };

                        if (iterationResult != null)
                        {
                            testResult.AddIteration(iterationResult);
                        }

                        // group?
                        var groupResult = graphRoot.SelectMany(r => r.TestGroups)
                            .SingleOrDefault(g => g.GroupName == group);

                        if (groupResult == null)
                        {
                            groupResult = new TestGroup
                            {
                                GroupName = group,
                                DurationTicks = 0,
                            };

                            groupResult.AddTestResult(testResult);

                            // assembly?
                            var testAssembly = graphRoot.SingleOrDefault(
                                a => a.AssemblyName == assembly);

                            if (testAssembly == null)
                            {
                                testAssembly = new TestAssembly
                                {
                                    AssemblyName = assembly,
                                    DurationTicks = 0,
                                };

                                testAssembly.AddGroup(groupResult);
                                graphRoot.Add(testAssembly);
                            }
                            else
                            {
                                testAssembly.AddGroup(groupResult);
                            }
                        }
                        else
                        {
                            groupResult.AddTestResult(testResult);
                        }
                    }

                    if (iterationResult != null)
                    {
                        testResult.AddIteration(iterationResult);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the name of the test.
        /// </summary>
        /// <param name="fullTest">The full test name.</param>
        /// <returns>The parsed parts of the test.</returns>
        private IList<string> TestNameParser(string fullTest)
        {
            var parsed = fullTest.AsSpan();
            var sentence = new List<string>();
            var iteration = string.Empty;
            var methodPos = parsed.IndexOf('(');
            if (methodPos > 0)
            {
                iteration = new string(parsed.Slice(methodPos).ToArray());
                parsed = parsed.Slice(0, methodPos);
            }

            var pos = parsed.LastIndexOf('.');
            var frontSegment = parsed.Slice(0, pos - 1);
            var testGroupPos = frontSegment.LastIndexOf('.');
            var assembly = frontSegment.Slice(0, testGroupPos);
            sentence.Add(new string(assembly.ToArray()));
            var testGroup = frontSegment.Slice(testGroupPos + 1);
            sentence.Add(new string(testGroup.ToArray()));
            var wordPos = ++pos;
            pos += 1;
            while (pos > 0 && pos < parsed.Length)
            {
                if (parsed[pos] >= 'A' && parsed[pos] <= 'Z')
                {
                    var length = pos - wordPos;
                    var word = new string(parsed.Slice(wordPos, length).ToArray());
                    sentence.Add(word);
                    wordPos = pos;
                }

                pos++;
            }

            sentence.Add(new string(parsed.Slice(wordPos, parsed.Length - wordPos).ToArray()));
            if (!string.IsNullOrWhiteSpace(iteration))
            {
                sentence.Add(iteration);
            }

            return sentence;
        }
    }
}
