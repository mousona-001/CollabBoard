using CollabBoard.Hubs;
using CollabBoard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRoomService, RoomService>();

// Configure CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();

// Map endpoints
app.MapGet("/", () => "CollabBoard API is running!");
app.MapGet("/test", () => new { message = "API is working", timestamp = DateTime.Now });
app.MapGet("/signalr-test", () => "SignalR Hub is configured at /drawingHub");

app.MapControllers();
app.MapHub<DrawingHub>("/drawingHub");

app.Run();
