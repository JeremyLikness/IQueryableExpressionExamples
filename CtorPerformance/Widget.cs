// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace CtorPerformance
{
    /// <summary>
    /// Widget class.
    /// </summary>
    public class Widget : IWidget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Widget"/> class.
        /// </summary>
        public Widget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Widget"/> class
        /// with parameters.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="guid">The unique id.</param>
        /// <param name="value">The value.</param>
        /// <param name="created">The created date.</param>
        public Widget(string id, Guid guid, int value, DateTime created)
        {
            Id = id;
            Unique = guid;
            Value = value;
            Created = created;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        public Guid Unique { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the created <see cref="DateTime"/>.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
