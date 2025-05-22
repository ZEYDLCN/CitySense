using CitySense.Data;
using CitySense.Domain.Interfaces;
using CitySense.Domain.Services;
using CitySense.Api.Services;
using CitySense.Api.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var FrontendDevelopmentOrigin = "_frontendDevelopmentOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: FrontendDevelopmentOrigin, policy =>
    {
        policy.WithOrigins("https://localhost:7267", "http://localhost:5267")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling =
            System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddScoped<IFakeDataOrchestrator, FakeDataOrchestrator>();
builder.Services.AddHostedService<FakeSensorService>();

builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(FrontendDevelopmentOrigin);

app.UseAuthorization();

app.MapControllers();
app.MapHub<SensorDataHub>("/sensorDataHub");

app.Run();
