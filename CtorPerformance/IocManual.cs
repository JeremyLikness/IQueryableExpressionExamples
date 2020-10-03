// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace CtorPerformance
{
    /// <summary>
    /// Version that uses <c>new</c>.
    /// </summary>
    public class IocManual : IService
    {
        /// <summary>
        /// Gets the widget.
        /// </summary>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The widget.</returns>
        public IWidget GetWidget(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return new Widget();
            }

            return new Widget(
                (string)parameters[0],
                (Guid)parameters[1],
                (int)parameters[2],
                (DateTime)parameters[3]);
        }
    }
}
