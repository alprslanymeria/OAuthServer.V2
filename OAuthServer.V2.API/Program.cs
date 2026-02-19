using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.API.ExceptionHandlers;
using OAuthServer.V2.API.Extensions;
using OAuthServer.V2.API.Filters;
using OAuthServer.V2.API.Middlewares;
using OAuthServer.V2.API.ModelBinding;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Data;
using OAuthServer.V2.Infrastructure;
using OAuthServer.V2.Infrastructure.OpenTelemetry;
using OAuthServer.V2.Service;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// OPEN TELEMETRY
builder.AddOpenTelemetryLogExt();

// SERVICES
builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new FileUploadModelBinderProvider());
});
builder.Services.AddOpenApi();

// HEALTH CHECKS
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("SqlServer")!,
        name: "sqlserver",
        tags: ["db", "ready"]);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? [];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services
    .AddRepositories(builder.Configuration)
    .AddServices()
    .AddInfrastructure(builder.Configuration)
    .AddCustomTokenAuth(builder.Configuration)
    .AddOpenTelemetryServicesExt(builder.Configuration);

// DISTRIBUTED CACHE FOR FIDO2 OPTIONS STORAGE
builder.Services.AddDistributedMemoryCache();

// FIDO2 / PASSKEYS
builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["Fido2:ServerDomain"];
    options.ServerName = builder.Configuration["Fido2:ServerName"];
    options.Origins = builder.Configuration.GetSection("Fido2:Origins").Get<HashSet<string>>();
    options.TimestampDriftTolerance = builder.Configuration.GetValue<int>("Fido2:TimestampDriftTolerance", 300000);
});


// FLUENT VALIDATION AUTO VALIDATION
builder.Services.AddFluentValidationAutoValidation(cfg =>
{
    cfg.OverrideDefaultResultFactoryWith<FluentValidationFilter>();
});

// EXCEPTION HANDLERS
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// OPTIONS PATTERN
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));
builder.Services.Configure<TokenOption>(builder.Configuration.GetSection("TokenOptions"));
builder.Services.Configure<TwilioOption>(builder.Configuration.GetSection(TwilioOption.Key));
builder.Services.Configure<SmtpOption>(builder.Configuration.GetSection(SmtpOption.Key));

var app = builder.Build();

app.UseExceptionHandler(x => { });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// HEALTH CHECK ENDPOINTS
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});



// MIDDLEWARES
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();