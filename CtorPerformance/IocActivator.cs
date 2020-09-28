using System;

namespace CtorPerformance
{
    public class IocActivator : IService
    {
        public IWidget GetWidget(params object[] parameters)
        {
            return (IWidget)Activator.CreateInstance(
                typeof(Widget),
                parameters);
        }
    }
}
