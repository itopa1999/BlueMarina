using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Middlewares;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application;
using BlueMarina.Infrastructure;
using BlueMarina.Shared.Common.Results;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace BlueMarina.Api;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;


    // Register services
    public void ConfigureServices(IServiceCollection services)
    {
        // Controllers
        services.AddControllers(options =>
            {
                options.Filters.Add<ApiVersionResultFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(
                    namingPolicy: JsonNamingPolicy.CamelCase,
                    allowIntegerValues: false));
            });

        services.AddHealthChecks();
        
        // CQRS (MediatR)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(AssemblyReference).Assembly
            );
            cfg.AddOpenBehavior(
                typeof(Application.Behaviors.RequestValidationBehavior<,>));
        });

        // Model Behaviour setting
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .Where(x => !string.IsNullOrWhiteSpace(x));

                var result = new BaseResult(
                    statusCode: HttpStatusCode.BadRequest,
                    message: string.Join(" | ", errors)
                );

                return new ObjectResult(result)
                {
                    StatusCode = 400
                };
            };
        });

        var origins = (_configuration["Cors:AllowedOrigins"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                policy =>
                {
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });

            options.AddPolicy("AllowAllDev",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        services.AddAppRateLimiting();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true; 
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        

        // Swagger
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BlueMarina API",
                Version = "v1",
                Description = "BlueMarina Fintech Platform API",

                Contact = new OpenApiContact
                {
                    Name = "BlueMarina Engineering Team",
                    Email = "engineering@bluemarina.com"
                },

                License = new OpenApiLicense
                {
                    Name = "Internal Use"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Token. Example: eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityDefinition("X-Platform", new OpenApiSecurityScheme
            {
                Name = "X-Platform",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Required platform identifier. Allowed values: Web, Mobile, Swagger"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                        
                    },
                    Array.Empty<string>()
                },

                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "X-Platform"
                        }
                    },
                    Array.Empty<string>()
                }
            });


            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "BlueMarina API",
                Version = "v2",
                Description = "BlueMarina Fintech Platform API V2"
            });


            options.SwaggerDoc("v3", new OpenApiInfo
            {
                Title = "BlueMarina API",
                Version = "v3",
                Description = "BlueMarina Fintech Platform API V3"
            });

            options.CustomSchemaIds(type =>
            {
                var fullName = type.FullName ?? type.Name;
                return fullName
                    .Replace("+", "_")
                    .Replace("`", "_")
                    .Replace("[", "_")
                    .Replace("]", "")
                    .Replace(",", "_")
                    .Replace(" ", "_");
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            options.IncludeXmlComments(xmlPath);

        });


        // Later add:
        services.AddInfrastructure(_configuration);
        services.Configure<SwaggerSettings>(
            _configuration.GetSection(SwaggerSettings.SectionName));
        services.Configure<HangfireSettings>(
            _configuration.GetSection(HangfireSettings.SectionName));
        
        services.AddDataProtection();
    }

    
    // Configure middleware pipeline
    public void Configure(WebApplication app)
    {
        // HangFire
        var recurringJobManager = app.Services
            .GetRequiredService<IRecurringJobManager>();

        // ExceptionsHandlerMiddleWare
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<PlatformValidationMiddleware>();


        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }


        app.UseCors("AllowSpecificOrigin");

        app.UseMiddleware<SwaggerAuthMiddleware>();

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.DisplayRequestDuration();

            options.EnablePersistAuthorization();

            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "BlueMarina API v1");


            options.SwaggerEndpoint(
                "/swagger/v2/swagger.json",
                "BlueMarina API v2");


            options.SwaggerEndpoint(
                "/swagger/v3/swagger.json",
                "BlueMarina API v3");


            options.DocumentTitle =
                "BlueMarina API Documentation";
        });

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseRateLimiter();

        app.UseMiddleware<HangfireAuthMiddleware>();

        app.UseHangfireDashboard("/hangfire");

        app.UseStaticFiles();

        app.MapHealthChecks("/health/live");

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.MapControllers();
    }

}