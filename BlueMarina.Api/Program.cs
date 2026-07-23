using BlueMarina.Api;
using BlueMarina.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

await app.Services.SeedIdentityAsync();

startup.Configure(app);

app.Run();