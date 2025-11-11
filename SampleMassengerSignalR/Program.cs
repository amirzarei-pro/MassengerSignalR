using Scalar.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SampleMassengerSignalR.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Built-in OpenAPI generation (Minimal OpenAPI)
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS (allow any origin, header, and method)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Ensure App_Data exists
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);

// Add LiteDB repo
builder.Services.AddSingleton<IChatRepository, LiteDbChatRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable CORS globally
app.UseCors("AllowAll");

// app.UseCors("Dev");

app.UseAuthorization();

// Serve static files for simple chat UI
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Map SignalR hubs
app.MapHub<SampleMassengerSignalR.Hubs.RealtimeHub>("/hubs/realtime");

app.Run();
