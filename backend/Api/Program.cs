global using FluentValidation;
using System;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Database;
using Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.Configure<HostOptions>(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(1));
builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();
builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.InitializeDatabase();

    app.UseOpenApi();
    app.UseSwaggerUi3(c =>
    {
        c.ConfigureDefaults();
    });
}

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

app.UseFastEndpoints(options =>
{
    options.Endpoints.Configurator = o => o.DontAutoTag();
    options.Endpoints.RoutePrefix = "api/v1";
} );

app.Run();