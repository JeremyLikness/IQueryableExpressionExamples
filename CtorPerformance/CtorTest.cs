// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace CtorPerformance
{
    /// <summary>
    /// Hosts the test code for the benchmark.
    /// </summary>
    public class CtorTest
    {
        private const int Ops = 100;

        private readonly IService baseline = new IocManual();
        private readonly IService activator = new IocActivator();
        private readonly IService expression = new IocExpression();

        private readonly object[] parameterArray = new object[]
        {
            new object[0],
            new object[]
            {
                nameof(CtorTest),
                Guid.NewGuid(),
                int.MaxValue,
                DateTime.Now,
            },
        };

        private readonly Func<IWidget, bool>[] validations;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtorTest"/> class.
        /// </summary>
        public CtorTest()
        {
            validations = new[]
            {
                (Func<IWidget, bool>)(widget => widget != null),
                widget => widget.Id == nameof(CtorTest) &&
                    widget.Unique == (Guid)((object[])parameterArray[1])[1] &&
                    widget.Value == int.MaxValue &&
                    widget.Created == (DateTime)((object[])parameterArray[1])[3],
            };

            // make sure each version will produce the right instance
            foreach (IService service in new IService[]
            {
                new IocManual(),
                new IocActivator(),
                new IocExpression(),
            })
            {
                for (var idx = 0; idx < parameterArray.Length; idx += 1)
                {
                    IWidget widget = null;
                    var parameters = parameterArray[idx] as object[];
                    switch (parameters.Length)
                    {
                        case 0:
                            widget = service.GetWidget();
                            break;
                        case 1:
                            widget = service.GetWidget(
                                parameters[0]);
                            break;
                        case 2:
                            widget = service.GetWidget(
                                parameters[0],
                                parameters[1]);
                            break;
                        case 3:
                            widget = service.GetWidget(
                                parameters[0],
                                parameters[1],
                                parameters[2]);
                            break;
                        case 4:
                            widget = service.GetWidget(
                                parameters[0],
                                parameters[1],
                                parameters[2],
                                parameters[3]);
                            break;
                    }

                    if (!validations[idx](widget))
                    {
                        var parms = string.Join(
                            ',',
                            (parameterArray[idx] as object[])
                            .Select(p => p.GetType().ToString())
                            .ToArray());
                        throw new InvalidOperationException(
                            $"{service.GetType()}: {parms}");
                    }
                }
            }
        }

        /// <summary>
        /// Manual way to create an instance: use <c>new</c>.
        /// </summary>
        [Benchmark(Baseline = true, Description = "new()", OperationsPerInvoke = Ops * 2)]
        public void Manual()
        {
            Iterate(baseline);
        }

        /// <summary>
        /// Create an instance using the activator.
        /// </summary>
        [Benchmark(Description = "Activator.CreateInstance", OperationsPerInvoke = Ops * 2)]
        public void Activator()
        {
            Iterate(activator);
        }

        /// <summary>
        /// Create an instancce using <c>Expression.New()</c>.
        /// </summary>
        [Benchmark(Description = "Expression.New", OperationsPerInvoke = Ops * 2)]
        public void ExpressionNew()
        {
            Iterate(expression);
        }

        private void Iterate(IService service)
        {
            var round = Ops;
            while (round-- > 0)
            {
                for (var idx = 0; idx < parameterArray.Length; idx++)
                {
                    var widget = service.GetWidget(parameterArray[idx] as object[]);
                    var foo = widget.Id;
                }
            }
        }
    }
}
