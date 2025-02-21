namespace NZ.Orz.Routing;

public class PriorityRouteDataList<T> : SortedList<int, T>
{
    public PriorityRouteDataList() : base(Comparer<int>.Default)
    {
    }
}
