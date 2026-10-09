using TrueCode.SharedKernel.Results;

namespace TrueCode.Finance.Application.Errors;

public static class CurrencyErrors
{
    public static Result<T> NotFound<T>(int id) => new Result<T>().AddError($"Валюта '{id}' не найдена.");

    public static Result<T> CodeTaken<T>(string code) => new Result<T>().AddError($"Валюта с кодом '{code}' уже существует.");

    public static Result<T> NameTaken<T>(string name) => new Result<T>().AddError($"Валюта '{name}' уже существует.");

    public static Result<T> Invalid<T>(string description) => new Result<T>().AddError(description);
}
