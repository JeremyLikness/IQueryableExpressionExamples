using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq;

namespace CtorPerformance
{
    public class CtorTest
    {
        private const int ops = 100;

        private readonly IService baseline = new IocManual();
        private readonly IService activator = new IocActivator();
        private readonly IService expression = new IocExpression();
        
        private readonly object[] parameterArray = new object[]
        {
            new object[0],
            new object[] { nameof(CtorTest), Guid.NewGuid(), int.MaxValue, DateTime.Now },
        };

        private readonly Func<IWidget, bool>[] validations;

        public CtorTest()
        {
            validations = new[]
            {
                (Func<IWidget, bool>)(widget => widget != null),
                widget => widget.Id == nameof(CtorTest) &&
                    widget.Unique == (Guid)((object[])parameterArray[1])[1] &&
                    widget.Value == int.MaxValue &&
                    widget.Created == (DateTime)((object[])parameterArray[1])[3]
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
                    switch(parameters.Length)
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

        private void Iterate(IService service)
        {
            var round = ops;
            while (round-- > 0)
            {
                for (var idx = 0; idx < parameterArray.Length; idx++)
                {
                    var widget = service.GetWidget(parameterArray[idx] as object[]);
                    var _ = widget.Id;
                }
            }
        }

        [Benchmark(Baseline = true, Description = "new()", OperationsPerInvoke = ops * 2)]
        public void Manual()
        {
            Iterate(baseline);
        }

        [Benchmark(Description = "Activator.CreateInstance", OperationsPerInvoke = ops * 2)]
        public void Activator()
        {
            Iterate(activator);
        }

        [Benchmark(Description = "Expression.New", OperationsPerInvoke = ops * 2)]
        public void ExpressionNew()
        {
            Iterate(expression);
        }        
    }

    class Program
    {
        static void Main(string[] _)
        {
            BenchmarkRunner.Run<CtorTest>();
        }
    }
}
