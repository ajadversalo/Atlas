using NetMinimal.Api.Endpoints;

namespace NetMinimal.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1").WithOpenApi();
        api.MapCategoryEndpoints();
        api.MapProductEndpoints();
        return app;
    }
}
