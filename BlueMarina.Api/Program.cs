using BlueMarina.Api;
using BlueMarina.Infrastructure.Identity;
using DotNetEnv;

var currentDirectory = Directory.GetCurrentDirectory();

var projectRoot = Path.GetFullPath(
    Path.Combine(
        currentDirectory,
        ".."
    )
);

var envPath = Path.Combine(
    projectRoot,
    ".env"
);

if (!File.Exists(envPath))
{
    throw new FileNotFoundException(
        $"Required .env file was not found at: {envPath}",
        envPath);
}

Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var wwwRootPath = Path.Combine(
    projectRoot,
    "wwwroot"
);

Directory.CreateDirectory(wwwRootPath);

builder.Environment.WebRootPath = wwwRootPath;

var startup = new Startup(
    builder.Configuration);

startup.ConfigureServices(
    builder.Services);

var app = builder.Build();

await app.Services.SeedIdentityAsync();

startup.Configure(app);

app.Run();