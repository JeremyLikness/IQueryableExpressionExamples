using System;
using System.Linq;
using System.Linq.Expressions;

namespace CtorPerformance
{
    public class IocExpression : IService
    {
        readonly Func<Widget> init;
        readonly Func<string, Guid, int, DateTime, Widget> initParams;

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
