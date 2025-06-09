using DemoAPI.Models;
using TokenEncryptor;

var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

if (!TokenEncryptorModul.checkEncryptedToken(appSettingsPath))
{
    Console.WriteLine("Something went wrong while encrypting the token");
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<ApiAuthentication>(builder.Configuration.GetSection("ApiAuthentication"));
builder.Services.AddHttpClient<VirtualMachineService>();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapControllers();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "api");
    });
}

//app.UseHttpsRedirection();

app.Run();

