using Microsoft.EntityFrameworkCore;
using TaskService;
using TaskService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseInMemoryDatabase("TaskServiceDb")
);

var app = builder.Build();

app.MapGrpcService<TaskServiceImpl>();
app.MapGet("/", () => "Hello from TaskService");

app.Run();
