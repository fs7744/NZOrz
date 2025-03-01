namespace NZ.Orz.Config;

public interface IRouteConfigValidator
{
    public ValueTask ValidateAsync(RouteConfig route, IList<Exception> errors, CancellationToken cancellationToken);
}