using System;

namespace CtorPerformance
{
    public class IocManual : IService
    {
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
