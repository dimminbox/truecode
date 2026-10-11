using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Errors;

public static class CurrencyErrors
{
    public static Result<T> NotFound<T>(string code) => new Result<T>().AddError($"Валюта '{code}' не найдена.");

    public static Result<T> Invalid<T>(string description) => new Result<T>().AddError(description);
}
