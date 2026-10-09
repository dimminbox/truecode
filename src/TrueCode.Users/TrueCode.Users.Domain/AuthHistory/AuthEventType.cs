namespace TrueCode.Users.Domain.AuthHistory;

public enum AuthEventType
{
    LoginSucceeded,
    LoginFailed,
    TokenRefreshed,
    LoggedOut,
}
