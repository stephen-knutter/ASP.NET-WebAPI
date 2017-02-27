using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class GenericAuthenticationFilter : AuthorizationFilterAttribute
    {
        /// <summary>
        /// Public default Constructor
        /// </summary>
        public GenericAuthenticationFilter()
        {
        }

        private readonly bool _isActive = true;

        /// <summary>
        /// parameter isActive explicitly enables/disables this filter
        /// </summary>
        /// <param name="isActive"></param>
        public GenericAuthenticationFilter(bool isActive)
        {
            _isActive = isActive;
        }

        /// <summary>
        /// Checks basic authentication request
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!_isActive) return;
            var identity = FetchAuthHeader(actionContext);
            if (identity == null)
            {
                ChallengeAuthRequest(actionContext);
                return;
            }
            var genericPrincipal = new GenericPrincipal(identity, null);
            Thread.CurrentPrincipal = genericPrincipal;
            if (!OnAuthorizeUser(identity.Name, identity.Password, actionContext))
            {
                ChallengeAuthRequest(actionContext);
                return;
            }

            base.OnAuthorization(actionContext);
        }

        /// <summary>
        /// Virtual method can be overrriden with the custom Authorization
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool OnAuthorizeUser(string user, string pass, HttpActionContext actionContext)
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                return false;
            return true;
        }

        /// <summary>
        /// Checks for authorization header in request, parses it, creates user credentials and returns Basic AuthenticationIdentity
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual BasicAuthenticationIdentity FetchAuthHeader(HttpActionContext actionContext)
        {
            string authHeaderValue = null;
            var authRequest = actionContext.Request.Headers.Authorization;
            if (authRequest != null && !String.IsNullOrEmpty(authRequest.Scheme) && authRequest.Scheme == "Basic")
                authHeaderValue = authRequest.Parameter;
            if (string.IsNullOrEmpty(authHeaderValue))
                return null;
            authHeaderValue = Encoding.Default.GetString(Convert.FromBase64String(authHeaderValue));
            var credentials = authHeaderValue.Split(':');
            return credentials.Length < 2 ? null : new BasicAuthenticationIdentity(credentials[0], credentials[1]);
        }

        /// <summary>
        /// Send the Authentication Challenge request
        /// </summary>
        /// <param name="actionContext"></param>
        private static void ChallengeAuthRequest(HttpActionContext actionContext)
        {
            var dnsHost = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", dnsHost));
        }
    }
}