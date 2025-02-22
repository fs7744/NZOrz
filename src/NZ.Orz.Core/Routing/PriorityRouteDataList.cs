namespace NZ.Orz.Routing;

public class PriorityRouteDataList<T> : SortedDictionary<int, List<T>>
{
    public PriorityRouteDataList() : base(Comparer<int>.Default)
    {
    }
}