using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Api;
using Api.Endpoints.CAA;
using Application.Interfaces;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Database;
using Infrastructure.Logging;
using Infrastructure.OpenApi;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();
builder.Services.Configure<HostOptions>(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(1));
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IJobNameUniquenessChecker, JobNameUniquenessChecker>();

// TODO(rg): into extension method
builder.Services.AddScoped<IJobNameUniquenessChecker, JobNameUniquenessChecker>();
builder.Services.AddScoped<ICHNameUniquenessChecker<Site>, SiteNameUniquenessChecker>();
builder.Services.AddScoped(typeof(ICHNameUniquenessChecker<,>), typeof(CHNameUniquenessChecker<,>));

// TODO(rg): into extension method
builder.Services.AddScoped<IDetectorConnection, DetectorHttpConnection>();
builder.Services.AddSingleton<IDetectorStreamCollection, DetectorStreamCollection>();
builder.Services.AddScoped<DetectorCommandService>();

//calibration service
builder.Services.AddScoped<DetectorCalibrationHttpService>();

builder.Services.AddHttpClient();

builder.Services.AddFastEndpoints();

builder.Services.AddSwaggerDoc(s =>
{
    s.TypeMappers = new[] { new PrimitiveTypeMapper(typeof(DetectorState), x => x.Type = JsonObjectType.String) };
    s.TypeNameGenerator = new ShorterTypeNameGenerator();
    s.SerializerSettings.Converters.Add(new StringEnumConverter());
    s.OperationProcessors.Add(new BinaryFormatOperationFilter());
    s.GenerateEnumMappingDescription = true;
    s.DocumentName = "v1";
});
builder.Services.AddSignalR();
builder.Services.AddCors();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes("StrONGKAutHENTICATIONKEy")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.Configure<DefaultUserOptions>(builder.Configuration.GetSection(DefaultUserOptions.ConfigurationEntryName));

builder.Services.AddSingleton<INotifyChannel, NotifyChannel>();
builder.Services.AddSingleton<IDetectorStatusService, DetectorStatusService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabase();
    app.UseOpenApi();
    app.UseSwaggerUi3(c => { c.ConfigureDefaults(); });
}
app.UseCors(options =>
{
    options.WithOrigins();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
    // NOTE(rg): workaround; the JS SignalR requires credentials to be allowed,
    // but AllowAnyOrigin and AllowCredentials can't be used together
    options.SetIsOriginAllowed(_ => true);
    options.AllowCredentials();
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
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
