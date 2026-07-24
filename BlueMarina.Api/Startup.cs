using System.Net;
using BlueMarina.Api.Middlewares;
using BlueMarina.Application;
using BlueMarina.Infrastructure;
using BlueMarina.Shared.Results;
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
        services.AddControllers();
        
        // CQRS (MediatR)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(AssemblyReference).Assembly
            );
        });

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
                if (type.IsNested)
                    return $"{type.DeclaringType!.Name}.{type.Name}";
                return type.Name;
            });

        });


        // Later add:
        services.AddInfrastructure(_configuration);
    }



    // Configure middleware pipeline
    public void Configure(WebApplication app)
    {

        if (app.Environment.IsDevelopment())
        {
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
        }


        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();
    }
}