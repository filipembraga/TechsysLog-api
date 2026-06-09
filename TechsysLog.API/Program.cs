using TechsysLog.CrossCutting;
using TechsysLog.Infrastructure.WebSockets.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();  
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication(builder.Configuration);
    
    
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<NotificationHub>("/hubs/notifications");
app.UseHttpsRedirection();

app.Run();
