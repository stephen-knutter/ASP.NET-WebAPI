using DataModel;
using DataModel.GenericRepository;
using DataModel.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace BusinessServices.Tests
{
    public class TokenServicesTests
    {
        private ITokenServices _tokenServices;
        private IUnitOfWork _unitOfWork;
        private List<Token> _tokens;
        private GenericRepository<Token> _tokenRepository;
        private WebApiDbEntities _dbEntities;

        public TokenServicesTests()
        {
            _tokens = SetUpTokens();
            _dbEntities = new Mock<WebApiDbEntities>().Object;
            _tokenRepository = SetUpTokenRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(s => s.TokenRepository).Returns(_tokenRepository);
            _unitOfWork = unitOfWork.Object;
            _tokenServices = new TokenServices(_unitOfWork);
        }

        private static List<Token> SetUpTokens()
        {
            var tokId = new int();
            var tokens = DataInitializer.GetAllTokens();
            foreach (Token tok in tokens)
                tok.TokenId = ++tokId;
            return tokens;
        }

        public void Dispose()
        {
            _tokens = null;
        }

        private GenericRepository<Token> SetUpTokenRepository()
        {
            var mockRepo = new Mock<GenericRepository<Token>>(MockBehavior.Default, _dbEntities);

            mockRepo.Setup(p => p.GetAll()).Returns(_tokens);

            mockRepo.Setup(p => p.GetByID(It.IsAny<int>()))
                .Returns(new Func<int, Token>(id => _tokens.Find(p => p.TokenId.Equals(id))));

            mockRepo.Setup(p => p.GetByID(It.IsAny<string>()))
                .Returns(new Func<string, Token>(authToken => _tokens.Find(p => p.AuthToken.Equals(authToken))));

            mockRepo.Setup(p => p.Insert(It.IsAny<Token>()))
                .Callback(new Action<Token>(newToken =>
                {
                    dynamic maxTokenID = _tokens.Last().TokenId;
                    dynamic nextTokenID = maxTokenID + 1;
                    newToken.TokenId = nextTokenID;
                    _tokens.Add(newToken);
                }));

            mockRepo.Setup(p => p.Update(It.IsAny<Token>()))
                .Callback(new Action<Token>(token =>
                {
                    var oldToken = _tokens.Find(a => a.TokenId == token.TokenId);
                    oldToken = token;
                }));

            mockRepo.Setup(p => p.Delete(It.IsAny<Token>()))
                .Callback(new Action<Token>(prod =>
                {
                    var tokenToRemove = _tokens.Find(a => a.TokenId == prod.TokenId);
                    if (tokenToRemove != null)
                        _tokens.Remove(tokenToRemove);
                }));

            return mockRepo.Object;
        }

        [Fact]
        public void GenerateTokenByUserIdTest()
        {
            const int userId = 1;
            var maxTokenIdBeforeAdd = _tokens.Max(a => a.TokenId);
            var tokenEntity = _tokenServices.GenerateToken(userId);
            var newTokenDataModel = new Token()
            {
                AuthToken = tokenEntity.AuthToken,
                TokenId = maxTokenIdBeforeAdd + 1,
                ExpiresOn = tokenEntity.ExpiresOn,
                IssuedOn = tokenEntity.IssuedOn,
                UserId = tokenEntity.UserId
            };
            Assert.Equal(newTokenDataModel.TokenId, _tokens.Last().TokenId);
        }
    }
}
