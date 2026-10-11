using System.Transactions;
using Moq;
using TrueCode.Finance.Application.Commands.ImportCurrencyRates;
using TrueCode.Finance.Domain.CurrencyAggregate;

namespace TrueCode.Finance.Application.Tests.Commands;

public class ImportCurrencyRatesCommandHandlerTests
{
    private readonly Mock<ICurrencyRepository> _currencyRepository = new();
    private readonly Mock<TimeProvider> _clock = new();
    private readonly ImportCurrencyRatesCommandHandler _handler;

    public ImportCurrencyRatesCommandHandlerTests()
    {
        _currencyRepository
            .Setup(r => r.BeginTransaction())
            .Returns(() => new TransactionScope(TransactionScopeAsyncFlowOption.Enabled));
        _clock.Setup(c => c.GetUtcNow()).Returns(CurrencyTestData.Now);
        _handler = new ImportCurrencyRatesCommandHandler(_currencyRepository.Object, _clock.Object);
    }

    private void SetupExisting(params Currency[] currencies) =>
        _currencyRepository
            .Setup(r => r.GetAllAsync(0, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

    [Fact]
    public async Task Handle_NewCurrency_AddsItWithCurrentTime()
    {
        const string code = "USD";
        const string name = "Доллар США";
        const decimal rate = 92.5m;
        SetupExisting();
        var command = new ImportCurrencyRatesCommand(new[] { new ImportCurrencyRateItem(code, name, rate) });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data!.Created);
        Assert.Equal(0, result.Data.Updated);
        Assert.Equal(0, result.Data.Unchanged);
        _currencyRepository.Verify(
            r => r.AddAsync(
                It.Is<Currency>(c =>
                    c.Code.Value == code &&
                    c.Name.Value == name &&
                    c.Rate.Value == rate &&
                    c.UpdatedAt == CurrencyTestData.Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RateChanged_UpdatesCurrency()
    {
        const string code = "USD";
        const string name = "Доллар США";
        const decimal oldRate = 90m;
        const decimal newRate = 92.5m;
        var existing = CurrencyTestData.Restore(1, code, name, oldRate);
        SetupExisting(existing);
        var command = new ImportCurrencyRatesCommand(new[] { new ImportCurrencyRateItem(code, name, newRate) });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data!.Created);
        Assert.Equal(1, result.Data.Updated);
        Assert.Equal(newRate, existing.Rate.Value);
        Assert.Equal(CurrencyTestData.Now, existing.UpdatedAt);
        _currencyRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _currencyRepository.Verify(
            r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RateNotChanged_DoesNotWrite()
    {
        const string code = "USD";
        const string name = "Доллар США";
        const decimal rate = 92.5m;
        var existing = CurrencyTestData.Restore(1, code, name, rate);
        SetupExisting(existing);
        var command = new ImportCurrencyRatesCommand(new[] { new ImportCurrencyRateItem(code, name, rate) });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data!.Unchanged);
        _currencyRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _currencyRepository.Verify(
            r => r.AddAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidItem_ReportsErrorAndImportsOthers()
    {
        SetupExisting();
        var command = new ImportCurrencyRatesCommand(new[]
        {
            new ImportCurrencyRateItem("EURO", "Евро", 100m),
            new ImportCurrencyRateItem("USD", "Доллар США", 92.5m)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.StartsWith("EURO:", result.Errors[0]);
        Assert.Equal(1, result.Data!.Created);
    }

    [Fact]
    public async Task Handle_TakesLockBeforeReadingCurrencies()
    {
        var sequence = new MockSequence();
        var repository = new Mock<ICurrencyRepository>(MockBehavior.Strict);
        
        repository.InSequence(sequence).Setup(r => r.BeginTransaction())
            .Returns(new TransactionScope(TransactionScopeAsyncFlowOption.Enabled));
        
        repository.InSequence(sequence).Setup(r => r.LockAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        repository.InSequence(sequence).Setup(r => r.GetAllAsync(0, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Currency>());
        var handler = new ImportCurrencyRatesCommandHandler(repository.Object, _clock.Object);

        var result = await handler.Handle(
            new ImportCurrencyRatesCommand(Array.Empty<ImportCurrencyRateItem>()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        repository.VerifyAll();
    }
}
