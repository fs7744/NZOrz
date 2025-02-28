using Microsoft.Extensions.Logging;
using NZ.Orz.Connections;

namespace NZ.Orz.ReverseProxy.L4;

public class L4ProxyMiddleware : IOrderMiddleware
{
    private readonly IL4Router router;
    private readonly ILogger<L4ProxyMiddleware> logger;

    public L4ProxyMiddleware(IL4Router router, ILogger<L4ProxyMiddleware> logger)
    {
        this.router = router;
        this.logger = logger;
    }

    public int Order => 0;

    public async Task Invoke(ConnectionContext context, ConnectionDelegate next)
    {
        try
        {
            var route = await router.MatchAsync(context);
            if (route == null)
            {
                logger.LogWarning($"No match route {context.LocalEndPoint}");
            }
            else
            {
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
        finally
        {
            await next(context);
        }
    }
}