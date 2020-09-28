namespace CtorPerformance
{
    public interface IService
    {
        IWidget GetWidget(params object[] parameters);
    }
}
