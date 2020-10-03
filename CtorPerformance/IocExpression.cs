// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;

namespace CtorPerformance
{
    /// <summary>
    /// Service that uses <see cref="NewExpression"/>.
    /// </summary>
    public class IocExpression : IService
    {
        /// <summary>
        /// Delegate for parameterless constructor.
        /// </summary>
        private readonly Func<Widget> init;

        /// <summary>
        /// Delegate for constructor with parameters.
        /// </summary>
        private readonly Func<string, Guid, int, DateTime, Widget> initParams;

        /// <summary>
        /// Initializes a new instance of the <see cref="IocExpression"/>
        /// class.
        /// </summary>
        public IocExpression()
        {
            var ctors = typeof(Widget).GetConstructors();
            var ctor = ctors.Where(c => c.GetParameters().Length == 0).Single();

            var ctorInit = Expression.New(ctor);
            var ctorLambda = Expression.Lambda<Func<Widget>>(
                ctorInit,
                new ParameterExpression[0]);
            init = ctorLambda.Compile();

            var ctorParams = ctors.Where(c => c.GetParameters().Length == 4).Single();
            var parameters = ctorParams.GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToArray();
            var ctorParamsInit = Expression.New(ctorParams, parameters);
            var ctorParamsLambda = Expression
                .Lambda<Func<string, Guid, int, DateTime, Widget>>(
                    ctorParamsInit,
                    parameters);
            initParams = ctorParamsLambda.Compile();
        }

        /// <summary>
        /// Get the widget.
        /// </summary>
        /// <param name="parameters">Constructor parameters.</param>
        /// <returns>The widget.</returns>
        public IWidget GetWidget(params object[] parameters)
        {
            var type = typeof(Widget);
            parameters ??= new object[0];

            if (parameters.Length == 0)
            {
                return init();
            }

            return initParams(
                (string)parameters[0],
                (Guid)parameters[1],
                (int)parameters[2],
                (DateTime)parameters[3]);
        }
    }
}
