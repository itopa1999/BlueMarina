using BlueMarina.Api;
using BlueMarina.Infrastructure.Identity;
using DotNetEnv;

var envPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    ".env"
);

Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

await app.Services.SeedIdentityAsync();

startup.Configure(app);

app.Run();