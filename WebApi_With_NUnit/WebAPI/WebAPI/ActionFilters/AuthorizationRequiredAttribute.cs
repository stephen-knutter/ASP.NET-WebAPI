using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BusinessServices;

namespace WebAPI.ActionFilters
{
    public class AuthorizationRequiredAttribute : ActionFilterAttribute
    {
        private const string Token = "Token";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            // Get API key provider
            var provider = actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(ITokenServices)) as ITokenServices;

            if (actionContext.Request.Headers.Contains(Token))
            {
                var tokenValue = actionContext.Request.Headers.GetValues(Token).First();

                // validate token
                if (provider != null && !provider.ValidateToken(tokenValue))
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        ReasonPhrase = "Invalid Request"
                    };
                    actionContext.Response = responseMessage;
                }
            }
            else
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            base.OnActionExecuting(actionContext);
        }
    }
}