using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessServices;
using WebAPI.Filters;

namespace WebAPI.Controllers
{
    [ApiAuthenticationFilter]
    public class AuthenticateController : ApiController
    {
        private readonly ITokenServices _tokenServices;

        #region Public constructor
        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        public AuthenticateController(ITokenServices tokenServices)
        {
            _tokenServices = tokenServices;
        }

        /// <summary>
        /// Public constructor to initialize product service instance
        /// </summary>
        /// <returns></returns>
        [Route("api/authenticate")]
        [Route("api/get/token")]
        [Route("api/login")]
        [HttpPost]
        public HttpResponseMessage Authenticate()
        {
            if (System.Threading.Thread.CurrentPrincipal != null &&
                System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var basicAuthenticationIdentity =
                    System.Threading.Thread.CurrentPrincipal.Identity as BasicAuthenticationIdentity;
                if (basicAuthenticationIdentity != null)
                {
                    var userId = basicAuthenticationIdentity.UserId;
                    return GetAuthToken(userId);
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid credentials");
        }

        /// <summary>
        /// Returns auth token for the validated user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private HttpResponseMessage GetAuthToken(int userId)
        {
            var token = _tokenServices.GenerateToken(userId);
            var response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
            response.Headers.Add("Token", token.AuthToken);
            response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["AuthTokenExpiry"]);
            response.Headers.Add("Access-Control-Expose-Headers", "Token, TokenExpiry");
            return response;
        }
        #endregion
    }
}