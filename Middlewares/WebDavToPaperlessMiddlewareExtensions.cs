namespace WebDavToPaperlessNGX.Middlewares;

public static class WebDavToPaperlessMiddlewareExtensions
{
    public static IApplicationBuilder UseWebDavToPaperless(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WebDavToPaperlessMiddleware>();
    }
}