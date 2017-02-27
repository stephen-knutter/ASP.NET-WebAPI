using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace WebAPI.Filters
{
    /// <summary>
    /// Basic Authentication identity
    /// </summary>
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public string Password { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }

        /// <summary>
        /// Basic Authentication Identity Constructor
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public BasicAuthenticationIdentity(string userName, string password) : base(userName, "Basic")
        {
            Password = password;
            UserName = userName;
        }
    }
}