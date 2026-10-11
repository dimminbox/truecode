using TrueCode.Finance.Domain.CurrencyAggregate;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Persistence;

namespace TrueCode.Finance.Infrastructure.Persistence.Repositories;

internal sealed class CurrencyRepository
    : Repository<Currency, CurrencyId, CurrencyRepository.CurrencyRow>, ICurrencyRepository
{
    public CurrencyRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    protected override string TableName => "currency";

    protected override string SelectColumns => "id, code, name, rate, updated_at AS UpdatedAt";

    protected override string InsertSql =>
        "INSERT INTO currency (code, name, rate, updated_at) VALUES (@Code, @Name, @Rate, @UpdatedAt) RETURNING id";

    protected override string UpdateSql =>
        "UPDATE currency SET rate = @Rate, updated_at = @UpdatedAt WHERE id = @Id";

    public override async Task<CurrencyId> AddAsync(Currency currency, CancellationToken ct = default) =>
        CurrencyId.Create(await ExecuteScalarAsync<int>(InsertSql, ToObject(currency), ct));

    public async Task<Currency?> GetByCodeAsync(CurrencyCode code, CancellationToken ct = default)
    {
        var row = await QuerySingleOrDefaultAsync<CurrencyRow>(
            $"SELECT {SelectColumns} FROM {TableName} WHERE code = @code", new { code = code.Value }, ct);
        return row == null ? null : ToDomainObject(row);
    }

    public async Task<IReadOnlyList<Currency>> GetAllAsync(int skip = 0, int take = 0, CancellationToken ct = default)
    {
        var limit = take > 0 ? " LIMIT @take" : "";
        var rows = await QueryAsync<CurrencyRow>(
            $"SELECT {SelectColumns} FROM {TableName} ORDER BY code{limit} OFFSET @skip",
            new { skip, take }, ct);
        return rows.Select(ToDomainObject).ToList();
    }

    protected override Currency ToDomainObject(CurrencyRow row) =>
        Currency.Restore(
            CurrencyId.Create(row.Id), CurrencyCode.Create(row.Code), CurrencyName.Create(row.Name), CurrencyRate.Create(row.Rate),
            new DateTimeOffset(row.UpdatedAt, TimeSpan.Zero));

    protected override object ToObject(Currency currency) => new
    {
        Id = currency.Id.Value,
        Code = currency.Code.Value,
        Name = currency.Name.Value,
        Rate = currency.Rate.Value,
        currency.UpdatedAt,
    };

    internal sealed record CurrencyRow(int Id, string Code, string Name, decimal Rate, DateTime UpdatedAt);
}
