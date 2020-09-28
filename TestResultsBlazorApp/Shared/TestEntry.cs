using System;
using System.Collections.Generic;
using System.Text;

namespace TestResultsBlazorApp.Shared
{
    public class TestEntry
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public long DurationTicks { get; set; }
        public string ParentId { get; set; }

        public TestEntry()
        {
        }

        public TestEntry(
            string id,
            string type,
            string name,
            long durationTicks,
            string parentId)
        {
            Id = id;
            Type = type;
            Name = name;
            DurationTicks = durationTicks;
            ParentId = parentId;
        }

        public override string ToString() =>
            $"{Type}({Id}): {Name}";

        public override int GetHashCode() =>
            HashCode.Combine(Type, Id);
    }
}
