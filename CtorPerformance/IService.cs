// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

namespace CtorPerformance
{
    /// <summary>
    /// Interface of service to get a widget.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Get the widget.
        /// </summary>
        /// <param name="parameters">The constructor parameters.</param>
        /// <returns>The widget.</returns>
        IWidget GetWidget(params object[] parameters);
    }
}
