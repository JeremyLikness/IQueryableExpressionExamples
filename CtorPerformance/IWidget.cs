using System;

namespace CtorPerformance
{
    public interface IWidget
    {
        string Id { get; }
        Guid Unique { get; }
        int Value { get; }
        DateTime Created { get; }
    }
}
