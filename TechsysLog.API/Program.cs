using TechsysLog.CrossCutting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();  
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();
    
    
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
