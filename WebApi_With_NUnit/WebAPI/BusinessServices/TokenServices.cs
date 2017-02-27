using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessEntities;
using DataModel;
using DataModel.UnitOfWork;
using System.Configuration;

namespace BusinessServices
{
    public class TokenServices : ITokenServices
    {
        private readonly UnitOfWork _unitOfWork;

        #region Public constructor
        /// <summary>
        /// Public constructor
        /// </summary>
        public TokenServices(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Public memeber methods
        /// <summary>
        /// Function to generate unique token with expiry against the provided userId
        /// Also add a record in database for generated token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TokenEntity GenerateToken(int userId)
        {
            string token = Guid.NewGuid().ToString();
            DateTime issuedOn = DateTime.Now;
            DateTime expiredOn = DateTime.Now.AddSeconds(
                Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
            var tokendomain = new Token
            {
                UserId = userId,
                AuthToken = token,
                IssuedOn = issuedOn,
                ExpiresOn = expiredOn
            };

            _unitOfWork.TokenRepository.Insert(tokendomain);
            _unitOfWork.Save();

            var tokenModel = new TokenEntity()
            {
                UserId = userId,
                IssuedOn = issuedOn,
                ExpiresOn = expiredOn,
                AuthToken = token
            };

            return tokenModel;
        }

        /// <summary>
        /// Function to validate token against expiry and existance in db
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public bool ValidateToken(string tokenId)
        {
            var token = _unitOfWork.TokenRepository.Get(t => t.AuthToken == tokenId && t.ExpiresOn > DateTime.Now);
            if (token != null && !(DateTime.Now > token.ExpiresOn))
            {
                token.ExpiresOn = token.ExpiresOn.AddSeconds(
                    Convert.ToDouble(ConfigurationManager.AppSettings["AuthTokenExpiry"]));
                _unitOfWork.TokenRepository.Update(token);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method to kill the provided token id.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        public bool Kill(string tokenId)
        {
            _unitOfWork.TokenRepository.Delete(x => x.AuthToken == tokenId);
            _unitOfWork.Save();
            var isNotDeleted = _unitOfWork.TokenRepository.GetMany(x => x.AuthToken == tokenId).Any();
            if (isNotDeleted) { return false; }
            return true;
        }

        public bool DeleteByUserId(int userId)
        {
            _unitOfWork.TokenRepository.Delete(x => x.UserId == userId);
            _unitOfWork.Save();
            var isNotDeleted = _unitOfWork.TokenRepository.GetMany(x => x.UserId == userId).Any();
            return !isNotDeleted;
        }
        #endregion
    }
}
