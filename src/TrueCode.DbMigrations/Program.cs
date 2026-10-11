using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Postgres.");

await using var provider = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(runner => runner
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(logging => logging.AddFluentMigratorConsole())
    .BuildServiceProvider(validateScopes: false);

const string DownCommand = "down";
const int ArgumentsErrorExitCode = 1;

var runner = provider.GetRequiredService<IMigrationRunner>();

if (args.Length == 0)
{
    runner.MigrateUp();
    return 0;
}

if (args.Length == 2 && args[0] == DownCommand && long.TryParse(args[1], out var version))
{
    runner.MigrateDown(version);
    return 0;
}

Console.Error.WriteLine($"Неизвестные аргументы. Использование: без аргументов - применить миграции, \"{DownCommand} <версия>\" - откатить до указанной версии.");
return ArgumentsErrorExitCode;
