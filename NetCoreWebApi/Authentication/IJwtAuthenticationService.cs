using NetCoreWebApi.Models;

namespace NetCoreWebApi.Authentication
{
    public interface IJwtAuthenticationService
    {
        string Authenticate(string authHeader, out WebApiUser webApiUser);
    }
}

