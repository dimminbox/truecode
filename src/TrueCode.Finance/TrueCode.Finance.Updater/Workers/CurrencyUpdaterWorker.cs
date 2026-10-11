using MediatR;
using Microsoft.Extensions.Options;
using TrueCode.Finance.Application.Commands.ImportCurrencyRates;
using TrueCode.Finance.Updater.Cbr;
using TrueCode.Finance.Updater.Options;

namespace TrueCode.Finance.Updater.Workers;

internal sealed class CurrencyUpdaterWorker : BackgroundService
{
    private static readonly TimeSpan ConfigRecheckDelay = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICbrRatesClient _ratesClient;
    private readonly IOptionsMonitor<UpdaterOptions> _options;
    private readonly ILogger<CurrencyUpdaterWorker> _logger;

    public CurrencyUpdaterWorker(IServiceScopeFactory scopeFactory, ICbrRatesClient ratesClient, IOptionsMonitor<UpdaterOptions> options, ILogger<CurrencyUpdaterWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _ratesClient = ratesClient;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!TryGetSettings(out var settings))
                {
                    _logger.LogWarning(
                        "Настройки Updater некорректны, повторная проверка через {Delay}.", ConfigRecheckDelay);
                    await Task.Delay(ConfigRecheckDelay, stoppingToken);
                    continue;
                }

                await UpdateAsync(settings, stoppingToken);
                await Task.Delay(settings.Interval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Получен сигнал остановки, обновление курсов валют завершено.");
        }
    }

    private bool TryGetSettings(out UpdaterOptions settings)
    {
        try
        {
            settings = _options.CurrentValue;
            return UpdaterOptions.IsValid(settings);
        }
        catch (InvalidOperationException)
        {
            settings = new UpdaterOptions();
            return false;
        }
    }

    private async Task UpdateAsync(UpdaterOptions settings, CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _ratesClient.GetRatesAsync(settings, cancellationToken);

            foreach (var error in rates.Errors)
                _logger.LogWarning("Данные ЦБ: {Error}", error);

            if (rates.Data == null || rates.Data.Count == 0)
            {
                _logger.LogWarning("Нет данных для сохранения, обновление пропущено.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var import = await sender.Send(new ImportCurrencyRatesCommand(rates.Data), cancellationToken);

            foreach (var error in import.Errors)
                _logger.LogWarning("Сохранение курсов: {Error}", error);

            _logger.LogInformation(
                "Курсы обновлены: создано {Created}, изменено {Updated}, без изменений {Unchanged}.",
                import.Data.Created, import.Data.Updated, import.Data.Unchanged);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Не удалось обновить курсы валют.");
        }
    }
}
