// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace CtorPerformance
{
    /// <summary>
    /// Interface for a widget.
    /// </summary>
    public interface IWidget
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        Guid Unique { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        int Value { get; }

        /// <summary>
        /// Gets the date created.
        /// </summary>
        DateTime Created { get; }
    }
}
