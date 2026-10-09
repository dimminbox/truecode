using System.Text;
using TrueCode.Finance.Application;
using TrueCode.Finance.Infrastructure;
using TrueCode.Finance.Updater.Cbr;
using TrueCode.Finance.Updater.Options;
using TrueCode.Finance.Updater.Workers;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<UpdaterOptions>(builder.Configuration.GetSection(UpdaterOptions.SectionName));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHttpClient<ICbrRatesClient, CbrRatesClient>();

builder.Services.AddHostedService<CurrencyUpdaterWorker>();

await builder.Build().RunAsync();