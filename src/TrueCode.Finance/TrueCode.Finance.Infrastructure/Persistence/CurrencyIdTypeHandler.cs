using System.Data;
using Dapper;
using TrueCode.Finance.Domain.CurrencyAggregate.ValueObjects;

namespace TrueCode.Finance.Infrastructure.Persistence;

internal sealed class CurrencyIdTypeHandler : SqlMapper.TypeHandler<CurrencyId>
{
    public override void SetValue(IDbDataParameter parameter, CurrencyId value)
    {
        parameter.DbType = DbType.Int32;
        parameter.Value = value.Value;
    }

    public override CurrencyId Parse(object value) => CurrencyId.Create(Convert.ToInt32(value));
}
