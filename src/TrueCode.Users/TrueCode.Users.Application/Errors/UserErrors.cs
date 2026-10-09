using TrueCode.SharedKernel.Results;

namespace TrueCode.Users.Application.Errors;

public static class UserErrors
{
    public static Result<T> InvalidCredentials<T>() => new Result<T>().AddError("Неверный логин или пароль.");

    public static Result<T> InvalidRefreshToken<T>() => new Result<T>().AddError("Недействительный refresh-токен.");

    public static Result<T> LoginTaken<T>(string login) => new Result<T>().AddError($"Логин '{login}' уже занят.");

    public static Result<T> Invalid<T>(string description) => new Result<T>().AddError(description);
}
