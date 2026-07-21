using BlueMarina.Api;


var builder = WebApplication.CreateBuilder(args);


var startup = new Startup(builder.Configuration);


// Register services
startup.ConfigureServices(builder.Services);


var app = builder.Build();


// Configure pipeline
startup.Configure(app);


app.Run();