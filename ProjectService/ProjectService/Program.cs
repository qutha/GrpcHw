using Microsoft.EntityFrameworkCore;
using ProjectService;
using ProjectService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseInMemoryDatabase("ProjectServiceDb")
);

var app = builder.Build();

app.MapGrpcService<ProjectServiceImpl>();
app.MapGet("/", () => "Hello from ProjectService");

app.Run();
