// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace TestResultsBlazorApp.Shared
{
    /// <summary>
    /// Generic test entry used by the client.
    /// </summary>
    public class TestEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEntry"/> class.
        /// </summary>
        public TestEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestEntry"/> class
        /// with properties.
        /// </summary>
        /// <param name="id">The id of the test.</param>
        /// <param name="type">The type of the test.</param>
        /// <param name="name">The name of the test.</param>
        /// <param name="durationTicks">The duration of the test.</param>
        /// <param name="parentId">The parent entity of the test.</param>
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

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public long DurationTicks { get; set; }

        /// <summary>
        /// Gets or sets the id of the parent.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Returns the string representation.
        /// </summary>
        /// <returns>The type, id and name of the entry.</returns>
        public override string ToString() =>
            $"{Type}({Id}): {Name}";

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash using combined type and id.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(Type, Id);
    }
}
