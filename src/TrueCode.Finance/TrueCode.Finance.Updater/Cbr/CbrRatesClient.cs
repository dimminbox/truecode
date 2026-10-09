using TrueCode.Finance.Application.Commands.ImportCurrencyRates;
using TrueCode.Finance.Updater.Options;
using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Updater.Cbr;

public interface ICbrRatesClient
{
    Task<Result<IReadOnlyList<ImportCurrencyRateItem>>> GetRatesAsync(UpdaterOptions settings, CancellationToken cancellationToken);
}

internal sealed class CbrRatesClient : ICbrRatesClient
{
    private const long MaxResponseBytes = 1_000_000;

    private readonly HttpClient _httpClient;

    public CbrRatesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<IReadOnlyList<ImportCurrencyRateItem>>> GetRatesAsync(
        UpdaterOptions settings, CancellationToken cancellationToken)
    {
        var result = new Result<IReadOnlyList<ImportCurrencyRateItem>> { Data = [] };

        using var ctsTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ctsTimeout.CancelAfter(settings.RequestTimeout);

        try
        {
            using var response = await _httpClient.GetAsync(settings.Url, HttpCompletionOption.ResponseHeadersRead, ctsTimeout.Token);

            if (!response.IsSuccessStatusCode)
                return result.AddError($"ЦБ вернул статус {(int)response.StatusCode}.");

            if (response.Content.Headers.ContentLength > MaxResponseBytes)
                return result.AddError("Ответ ЦБ превышает допустимый размер.");

            var bytes = await response.Content.ReadAsByteArrayAsync(ctsTimeout.Token);
            if (bytes.Length > MaxResponseBytes)
                return result.AddError("Ответ ЦБ превышает допустимый размер.");

            using var stream = new MemoryStream(bytes);
            return CbrXmlParser.Parse(stream);
        }
        catch (HttpRequestException ex)
        {
            return result.AddError($"Ошибка запроса к ЦБ: {ex.Message}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return result.AddError("Истёк таймаут запроса к ЦБ.");
        }
    }
}
