using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;
using TrueCode.SharedKernel.Domain;

namespace TrueCode.Finance.Domain.CurrencyAggregate;

public interface ICurrencyRepository : IRepository<Currency, CurrencyId>
{
    Task<IReadOnlyList<Currency>> GetAllAsync(int skip = 0, int take = 0, CancellationToken ct = default);

    Task<bool> ExistsByCodeAsync(CurrencyCode code, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(CurrencyName name, CancellationToken ct = default);

    Task DeleteAsync(Currency currency, CancellationToken ct = default);
}