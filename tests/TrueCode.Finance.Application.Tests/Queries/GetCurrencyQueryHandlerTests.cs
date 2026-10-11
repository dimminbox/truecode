using Moq;
using TrueCode.Finance.Application.Queries.GetCurrency;
using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

namespace TrueCode.Finance.Application.Tests.Queries;

public class GetCurrencyQueryHandlerTests
{
    private readonly Mock<ICurrencyRepository> _currencyRepository = new();
    private readonly GetCurrencyQueryHandler _handler;

    public GetCurrencyQueryHandlerTests()
    {
        _handler = new GetCurrencyQueryHandler(_currencyRepository.Object);
    }

    [Fact]
    public async Task Handle_CurrencyExists_ReturnsCurrency()
    {
        var currency = CurrencyTestData.Restore(1, "USD", "Доллар США", 92.5m);
        _currencyRepository
            .Setup(r => r.GetByCodeAsync(CurrencyCode.Create("USD"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        var result = await _handler.Handle(new GetCurrencyQuery("usd"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("USD", result.Data.Code);
        Assert.Equal("Доллар США", result.Data.Name);
        Assert.Equal(92.5m, result.Data.Rate);
        Assert.Equal(currency.UpdatedAt, result.Data.UpdatedAt);
    }

    [Fact]
    public async Task Handle_CurrencyNotFound_ReturnsError()
    {
        _currencyRepository
            .Setup(r => r.GetByCodeAsync(It.IsAny<CurrencyCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        var result = await _handler.Handle(new GetCurrencyQuery("EUR"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains("EUR", result.GetErrorsString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("US1")]
    public async Task Handle_InvalidCode_ReturnsErrorWithoutRepositoryCall(string code)
    {
        var result = await _handler.Handle(new GetCurrencyQuery(code), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _currencyRepository.Verify(
            r => r.GetByCodeAsync(It.IsAny<CurrencyCode>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
