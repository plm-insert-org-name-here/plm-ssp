global using FluentValidation;
using System.Text.Json.Serialization;
using Api;
using Api.Endpoints.Detectors;
using Application.Interfaces;
using Application.Services;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Services;
using FastEndpoints;
using FastEndpoints.ClientGen;
using FastEndpoints.Swagger;
using Infrastructure.Database;
using Infrastructure.Logging;
using Infrastructure.OpenApi;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();
builder.Services.Configure<HostOptions>(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(1));
builder.Services.AddDatabase(builder.Configuration);

// TODO(rg): into extension method
builder.Services.AddScoped<ICHNameUniquenessChecker<Site>, SiteNameUniquenessChecker>();
builder.Services.AddScoped(typeof(ICHNameUniquenessChecker<,>), typeof(CHNameUniquenessChecker<,>));

// TODO(rg): into extension method
builder.Services.AddScoped<IDetectorConnection, DetectorHttpConnection>();
builder.Services.AddSingleton<IDetectorStreamCollection, DetectorStreamCollection>();
builder.Services.AddScoped<DetectorCommandService>();

builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

builder.Services.AddFastEndpoints();

builder.Services.AddSwaggerDoc(s =>
{
    s.SerializerSettings.Converters.Add(new StringEnumConverter());
    s.GenerateEnumMappingDescription = true;
    s.DocumentName = "Version 1";
});
builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabase();
    app.UseOpenApi();
    app.UseSwaggerUi3(c => { c.ConfigureDefaults(); });
}
app.MapTypeScriptClientEndpoint("/ts-client", "Version 1", s =>
{
    s.ClassName = "ApiClient";
    s.TypeScriptGeneratorSettings.Namespace = "ApiClient";
    s.TypeScriptGeneratorSettings.TypeNameGenerator = new ShorterTypeNameGenerator();
    s.OperationNameGenerator = new ShorterOperationNameGenerator();
});
app.UseCors(options =>
{
    options.AllowAnyMethod();
    options.AllowAnyHeader();
    // NOTE(rg): workaround; the JS SignalR requires credentials to be allowed,
    // but AllowAnyOrigin and AllowCredentials can't be used together
    options.SetIsOriginAllowed(_ => true);
    // options.AllowAnyOrigin();
    options.AllowCredentials();
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseWebSockets();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseFastEndpoints(options =>
{
    options.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    options.Endpoints.Configurator = o =>
    {
        o.DontAutoTag();
        o.DontCatchExceptions();
    };
    options.Endpoints.RoutePrefix = "api/v1";
});

app.Run();