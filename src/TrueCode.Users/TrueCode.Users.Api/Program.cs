using TrueCode.Users.Api;
using TrueCode.Users.Application;
using TrueCode.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseApi();

app.Run();