using TrueCode.Finance.Api;
using TrueCode.Finance.Application;
using TrueCode.Finance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseApi();

app.Run();
