using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using NetCoreWebApi.DAL;
using NetCoreWebApi.Extensions;
using NetCoreWebApi.Models;
using NetCoreWebApi.ModelSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace NetCoreWebApi.Authentication
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        public static readonly string XAuthHeader = "X-AuthJwtToken";
        public const string SigningKey = "iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAABhGlDQ1BJQ0MgcHJvZmlsZQAAKJF9kT1Iw1AUhU/TlopUHewg4pChOlkQFXGUKhbBQmkrtOpg8tI/aNKQpLg4Cq4FB38Wqw4uzro6uAqC4A+Im5uToouUeF9SaBHjg8f7OO+dw733AUKzylQzMAGommWkE3Exl18VQ68IIoB+AOMSM/VkZjELz/V1Dx8/72I8y/vdn6tPKZgM8InEc0w3LOIN4plNS+e8TxxhZUkhPuf1GFQg8SPXZZffOJccFnhmxMim54kjxGKpi+UuZmVDJZ4mjiqqRvlCzmWF8xZntVpn7Tp5h+GCtpLhOu0RJLCEJFIQIaOOCqqwEKNTI8VEmu7jHv5hx58il0yuChg5FlCDCsnxg//B79maxalJNykcB4Ivtv0xCoR2gVbDtr+Pbbt1AvifgSut4681gdlP0hsdLXoEDGwDF9cdTd4DLneAoSddMiRH8tMWikXg/Yy+KQ8M3gK9a+7c2vc4fQCyNKvlG+DgEBgrUfa6R9893XP79017fj8z5nKOMIAmiwAAAAlwSFlzAAAuIwAALiMBeKU/dgAAAAd0SU1FB+UFBA4CFO+to0MAAAAZdEVYdENvbW1lbnQAQ3JlYXRlZCB3aXRoIEdJTVBXgQ4XAAABGklEQVQY0wXBDU+CQBgA4OM9DhWOOxWbQzZTIGWVrP//M4rN/FhrKplHKtqqw0PtebR7Cnd+oCMU+L35eCK2gnKrBFSzqR8NoNvtAtY8zxs9PK7SBSGkKApVnKIo2m23cMz3gLSO6yZJYprmev0tf/9c1yWECCFAKeU4DULw23xaZ1xHaDgcDoIw2whObWDUMnQ9eX4xTVMIEYbtp1G8XLxnm43ndnQppVJqNpsYgKNBaFkW0WE2mbJWyzCqcL4ojDXbtqWUcRxTSve7g2FUOW+UJ4VveaVCjLMq/V4/XaYYwyHP00/htG+AVHATzozVnabTqPPp+BU0fMiPFmdt16tRG64a7gf+FcFXtsvzn9XqQ12ulDJK2UmV/3w2dtqk4iw7AAAAAElFTkSuQmCC";
        public static NetCoreApiSettings webApiSettings = new NetCoreApiSettings();
        private NetCoreWebApiDbContext dbContext;

        private static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Convert.FromBase64String(JwtAuthenticationService.SigningKey);
                DateTime now = DateTime.UtcNow;
                var validationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    LifetimeValidator = (notBefore, expires, securityToken, vParameters) => expires >= now,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                return principal;
            }

            catch (Exception)
            {
                return null;
            }
        }

        public JwtAuthenticationService(IOptions<NetCoreApiSettings> settings, NetCoreWebApiDbContext dbContext)
        {
            webApiSettings = settings.Value;
            this.dbContext = dbContext;
        }

        public string Authenticate(string authHeader, out WebApiUser webApiUser)
        {
            string[] authData = authHeader.Replace("Bearer ", string.Empty).Decrypt().Split(':');
            string username = authData.Length > 1 ? authData[0]: string.Empty,
                   password = authData.Length > 1 ? authData[1].Sha512() : string.Empty;
                   
            webApiUser = this.dbContext.WebApiUsers.FirstOrDefault(u => u.Login == username && u.Password == password);
            
            if (webApiUser == null)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = UTF8Encoding.UTF8.GetBytes(JwtAuthenticationService.SigningKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddSeconds(webApiSettings.Authentication.TokenLifeTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static string RefreshToken(string oldToken)
        {            
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = UTF8Encoding.UTF8.GetBytes(SigningKey);
                var tokenParts = oldToken.Split('.');
                if (tokenParts?.Length == 3)
                {
                    string tokenDecoded = string.Empty;
                    try
                    {
                        tokenDecoded = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1]));
                        JsonConvert.DeserializeObject(tokenDecoded);
                    }
                    catch
                    {
                        try
                        {
                            tokenDecoded = UTF8Encoding.UTF8.GetString(Convert.FromBase64String($"{tokenParts[1]}="));
                            JsonConvert.DeserializeObject(tokenDecoded);
                        }
                        catch
                        {
                            tokenDecoded = UTF8Encoding.UTF8.GetString(Convert.FromBase64String($"{tokenParts[1]}=="));
                        }

                    }

                    int expireTime = webApiSettings.Authentication.TokenLifeTimeInSeconds;

                    dynamic tokenData = JsonConvert.DeserializeObject(tokenDecoded);
                    string userName = tokenData.unique_name ?? tokenData.name;

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, userName)}),
                        Expires = DateTime.UtcNow.AddSeconds(expireTime),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    return tokenHandler.WriteToken(token);
                }
                
            }
            catch
            {
                
            }

            return string.Empty;
        }
    }
}

