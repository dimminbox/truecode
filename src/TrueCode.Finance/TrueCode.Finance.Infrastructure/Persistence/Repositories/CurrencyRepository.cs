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
        CurrencyId.Create(await ExecuteScalarAsync<int>(InsertSql, ToParameters(currency), ct));

    public async Task<IReadOnlyList<Currency>> GetAllAsync(int skip = 0, int take = 0, CancellationToken ct = default)
    {
        var limit = take > 0 ? " LIMIT @take" : "";
        var rows = await QueryAsync<CurrencyRow>(
            $"SELECT {SelectColumns} FROM {TableName} ORDER BY code{limit} OFFSET @skip",
            new { skip, take }, ct);
        return rows.Select(ToDomain).ToList();
    }

    public async Task<bool> ExistsByCodeAsync(CurrencyCode code, CancellationToken ct = default) =>
        await ExecuteScalarAsync<bool>(
            $"SELECT EXISTS (SELECT 1 FROM {TableName} WHERE code = @code)", new { code = code.Value }, ct);

    public async Task<bool> ExistsByNameAsync(CurrencyName name, CancellationToken ct = default) =>
        await ExecuteScalarAsync<bool>(
            $"SELECT EXISTS (SELECT 1 FROM {TableName} WHERE name = @name)", new { name = name.Value }, ct);

    public async Task DeleteAsync(Currency currency, CancellationToken ct = default) =>
        await ExecuteAsync($"DELETE FROM {TableName} WHERE id = @id", new { id = currency.Id.Value }, ct);

    protected override Currency ToDomain(CurrencyRow row) =>
        Currency.Restore(
            CurrencyId.Create(row.Id), CurrencyCode.Create(row.Code), CurrencyName.Create(row.Name), CurrencyRate.Create(row.Rate),
            row.UpdatedAt);

    protected override object ToParameters(Currency currency) => new
    {
        Id = currency.Id.Value,
        Code = currency.Code.Value,
        Name = currency.Name.Value,
        Rate = currency.Rate.Value,
        currency.UpdatedAt,
    };

    internal sealed record CurrencyRow(int Id, string Code, string Name, decimal Rate, DateTimeOffset UpdatedAt);
}
