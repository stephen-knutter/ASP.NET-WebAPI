using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessEntities;

namespace BusinessServices
{
    public interface ITokenServices
    {
        #region Interface memeber methods.
        /// <summary>
        /// Function to generate unique token with expiry against the provided userId
        /// Also add a record in database for generated token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        TokenEntity GenerateToken(int userId);

        /// <summary>
        /// Function to validate token against expiry and existance in db
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        bool ValidateToken(string tokenId);

        /// <summary>
        /// Method to kill the provided token id.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        bool Kill(string tokenId);

        /// <summary>
        /// Delete tokens for the specific deleted user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool DeleteByUserId(int userId);
        #endregion
    }
}
