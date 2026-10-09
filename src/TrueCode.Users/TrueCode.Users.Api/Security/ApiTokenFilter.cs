using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrueCode.Users.Api.Security;

public sealed class ApiTokenFilter : IAuthorizationFilter
{
    private readonly string _token;

    public ApiTokenFilter(string token)
    {
        _token = token;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var provided = context.HttpContext.Request.Headers[ApiTokenDefaults.HeaderName].ToString();

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(_token)))
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}
