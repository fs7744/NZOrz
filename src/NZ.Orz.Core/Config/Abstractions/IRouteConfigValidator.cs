namespace NZ.Orz.Config;

public interface IRouteConfigValidator
{
    int Order { get; }

    public ValueTask ValidateAsync(RouteConfig route, IList<Exception> errors, CancellationToken cancellationToken);
}