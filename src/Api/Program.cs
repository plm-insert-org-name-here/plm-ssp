using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Api;
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

builder.Services.AddHttpClient();

builder.Services.AddFastEndpoints();

builder.Services.AddSwaggerDoc(s =>
{
    // NOTE(rg): DetectorState is a "bitfield" enum which is serialized into a comma-separated string
    // if multiple fields are set. As far as I can tell, the generated typescript-fetch API client
    // does not handle bitfields at all, and generates a regular DetectorState enum no matter what. Making it serialize
    // into a regular string and then "manually" converting into a DetectorState array feels more correct to me
    s.TypeMappers = new[] { new PrimitiveTypeMapper(typeof(DetectorState), x => x.Type = JsonObjectType.String) };
    s.TypeNameGenerator = new ShorterTypeNameGenerator();
    s.SerializerSettings.Converters.Add(new StringEnumConverter());
    s.OperationProcessors.Add(new BinaryFormatOperationFilter());
    s.GenerateEnumMappingDescription = true;
    s.DocumentName = "v1";
    // s.AddSecurity("oauth2", new OpenApiSecurityScheme
    // {
    //     Description = "Standard Authorization header using the Bearer scheme (\"{token}\")",
    //     Name = "Authorization"
    // });
});
builder.Services.AddSignalR();
builder.Services.AddCors();

// var jwtOptions = new JwtOptions();
// builder.Configuration.GetSection(JwtOptions.ConfigurationEntryName).Bind(jwtOptions);

// builder.Services.AddIdentityCore<User>(opt =>
// {
//     opt.Password.RequiredLength = 8;
//     opt.Password.RequireDigit = false;
//     opt.Password.RequireLowercase = false;
//     opt.Password.RequireUppercase = false;
//     opt.Password.RequireNonAlphanumeric = false;
// })
//     .AddRoles<ApplicationRole>()
//     //nemtom erre mi az új solution, edit: ig ez
//     .AddEntityFrameworkStores<Context>();

// JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
//
// var tokenValidationParameters = new TokenValidationParameters
// {
//     // ValidIssuer = jwtOptions.ValidIssuer,
//     // ValidAudience = jwtOptions.ValidAudience,
//     // NameClaimType = JwtRegisteredClaimNames.Sub,
//     // RoleClaimType = jwtOptions.RoleClaimName,
//     ValidateIssuerSigningKey = true,
//     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
//     ValidateIssuer = false,
//     ValidateAudience = false
// };

// builder.Services.AddAuthentication(opt =>
//     {
//         // TODO(rg): not all of these are needed
//         //ez a todo örökké itt lesz
//         opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//         opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//         opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//         opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
//     })
//     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "DefaultScheme", opt =>
//     {
//         opt.TokenValidationParameters = tokenValidationParameters;
//     });
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
// builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.ConfigurationEntryName));
// builder.Services.AddSingleton<IOptions<TokenValidationParameters>>(new OptionsWrapper<TokenValidationParameters>(tokenValidationParameters));

//bg service test
builder.Services.AddSingleton<INotifyChannel, NotifyChannel>();

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
    // options.AllowAnyOrigin();
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
