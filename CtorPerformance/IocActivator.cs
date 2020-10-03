// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;

namespace CtorPerformance
{
    /// <summary>
    /// Version of service that uses te <see cref="Activator"/>.
    /// </summary>
    public class IocActivator : IService
    {
        /// <summary>
        /// Gets the widget.
        /// </summary>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The instance.</returns>
        public IWidget GetWidget(params object[] parameters)
        {
            return (IWidget)Activator.CreateInstance(
                typeof(Widget),
                parameters);
        }
    }
}
