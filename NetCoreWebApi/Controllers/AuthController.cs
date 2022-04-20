using System;
using System.Linq;
using NetCoreWebApi.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using NetCoreWebApi.ModelSettings;
using NetCoreWebApi.DAL;
using NetCoreWebApi.Controllers.Common;
using NetCoreWebApi.Models;

namespace NetCoreWebApi.Controllers
{    
    [ApiController, Route("api/[controller]"), SwaggerTag("Controller for authentication management")]
    public class AuthController : ConnectedController
    {
        public AuthController(IOptions<NetCoreApiSettings> settings, NetCoreWebApiDbContext contextDb) : base(settings, contextDb)
        {
        }

        /// <summary>
        /// Runs initial authentication, with which to obtain an access authorization token
        /// </summary>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        [SwaggerResponse(200, "If authenticated. Also returns a Header <b>X-AuthJwtToken</b> with auth token")]
        [SwaggerResponse(401, "If not authenticaded properly.")]
        public IActionResult Get()
        {            
            var authHeader = this.Request.Headers[HeaderNames.Authorization].FirstOrDefault() ?? String.Empty;
            if (authHeader.StartsWith("Bearer"))
            {
                string token = new JwtAuthenticationService(iWebApiSettings, this.dbContext).Authenticate(authHeader, out WebApiUser authUser);
                if (!string.IsNullOrEmpty(token))
                {
                    this.Response.Headers.Add(JwtAuthenticationService.XAuthHeader, token);
                    return Ok(authUser);
                }                
            }

            return Unauthorized();
        }

    }
}

