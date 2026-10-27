using NetMinimal.Api.Extensions;
using NetMinimal.Infrastructure.StaticServices;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogServices(builder.Configuration);

var app = builder.Build();

AppServices.Initialize(app.Services);
await app.Services.InitializeDatabaseAsync();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("NetMinimal Catalog API");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription();
app.MapApiEndpoints();

app.Run();

public partial class Program;
