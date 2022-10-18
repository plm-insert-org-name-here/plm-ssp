using System;
using System.Reflection;
using System.Text.Json.Serialization;
using Api;
using Application;
using Application.Infrastructure.Database;
using Application.Infrastructure.Logging;
using Application.Services;
using Application.Services.DetectorController;
using Application.Services.MonitoringHandler;
using Application.Services.ProcessorHandler;
using Application.Services.StreamHandler;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Services.Configure<HostOptions>(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(1));

builder.Services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(configuration =>
        {
            configuration.EnableAnnotations();
            configuration.CustomSchemaIds(x => x.FullName);
        });

builder.Services.AddSingleton<SnapshotCache>();
builder.Services.AddSingleton<MonitoringHandler>();
builder.Services.AddDetectorControllerWebSocket(builder.Configuration);
builder.Services.AddStreamHandler(builder.Configuration);
builder.Services.AddProcessorHandler(builder.Configuration);

builder.Services.AddSignalR();

var app = builder.Build();

app.InitializeDatabase();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLM API V1");
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

var mapperConfiguration = app.Services.GetRequiredService<IConfigurationProvider>();
mapperConfiguration.AssertConfigurationIsValid();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseWebSockets();
app.UseDetectorControllerWebSocket();

app.UseEndpoints(options =>
{
    options.MapControllers();

    options.MapHub<StreamHub>(Routes.Detectors.DetectorHub);
} );

app.Run();