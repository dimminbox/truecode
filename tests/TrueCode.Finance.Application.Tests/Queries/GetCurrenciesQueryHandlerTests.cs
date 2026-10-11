using Moq;
using TrueCode.Finance.Application.Queries.GetCurrencies;
using TrueCode.Finance.Domain.CurrencyAggregate;

namespace TrueCode.Finance.Application.Tests.Queries;

public class GetCurrenciesQueryHandlerTests : IClassFixture<CurrenciesFixture>
{
    private readonly CurrenciesFixture _fixture;
    private readonly Mock<ICurrencyRepository> _currencyRepository = new();
    private readonly GetCurrenciesQueryHandler _handler;

    public GetCurrenciesQueryHandlerTests(CurrenciesFixture fixture)
    {
        _fixture = fixture;
        _handler = new GetCurrenciesQueryHandler(_currencyRepository.Object);
    }

    [Fact]
    public async Task Handle_CurrenciesExist_ReturnsMappedList()
    {
        var usd = CurrencyTestData.Restore(1, "USD", "Доллар США", 92.5m);
        var eur = CurrencyTestData.Restore(2, "EUR", "Евро", 100.25m);
        _currencyRepository
            .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { usd, eur });

        var result = await _handler.Handle(new GetCurrenciesQuery(1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("USD", result.Data[0].Code);
        Assert.Equal("Доллар США", result.Data[0].Name);
        Assert.Equal(92.5m, result.Data[0].Rate);
        Assert.Equal(usd.UpdatedAt, result.Data[0].UpdatedAt);
        Assert.Equal("EUR", result.Data[1].Code);
    }

    [Fact]
    public async Task Handle_NoCurrencies_ReturnsEmptyList()
    {
        _currencyRepository
            .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Currency>());

        var result = await _handler.Handle(new GetCurrenciesQuery(1, 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Theory]
    [InlineData(1, 10, 0, 10)]
    [InlineData(2, 10, 10, 10)]
    [InlineData(3, 25, 50, 25)]
    [InlineData(1, 100, 0, 100)]
    public async Task Handle_Page_PassesSkipAndTakeToRepository(int page, int pageSize, int expectedSkip, int expectedTake)
    {
        var currency = CurrencyTestData.Restore(1, "USD", "Доллар США", 92.5m);
        _currencyRepository
            .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { currency });

        var result = await _handler.Handle(new GetCurrenciesQuery(page, pageSize), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Data!);
        Assert.Equal("USD", item.Code);
        Assert.Equal(92.5m, item.Rate);
        _currencyRepository.Verify(
            r => r.GetAllAsync(expectedSkip, expectedTake, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 10, 10)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 10, 5)]
    [InlineData(4, 10, 0)]
    [InlineData(1, 100, CurrenciesFixture.TotalCount)]
    public async Task Handle_Page_ReturnsExpectedNumberOfItems(int page, int pageSize, int expectedCount)
    {
        _currencyRepository
            .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int skip, int take, CancellationToken ct) =>
                (IReadOnlyList<Currency>)_fixture.Currencies.Skip(skip).Take(take).ToList());

        var result = await _handler.Handle(new GetCurrenciesQuery(page, pageSize), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedCount, result.Data.Count);
        var expectedCodes = _fixture.Currencies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => c.Code.Value);
        Assert.Equal(expectedCodes, result.Data.Select(c => c.Code));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, 101)]
    public async Task Handle_InvalidPaging_ReturnsErrorWithoutRepositoryCall(int page, int pageSize)
    {
        var result = await _handler.Handle(new GetCurrenciesQuery(page, pageSize), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        _currencyRepository.Verify(
            r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
