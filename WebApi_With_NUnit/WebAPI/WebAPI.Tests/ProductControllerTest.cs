using BusinessServices;
using DataModel;
using DataModel.GenericRepository;
using DataModel.UnitOfWork;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using WebAPI.Controllers;
using System.Web.Http;
using Newtonsoft.Json;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace WebAPI.Tests
{
    public class ProductControllerTest
    {
        private IProductServices _productService;
        private ITokenServices _tokenService;
        private IUnitOfWork _unitOfWork;
        private List<Product> _products;
        private List<Token> _tokens;
        private GenericRepository<Product> _productRepository;
        private GenericRepository<Token> _tokenRepository;
        private WebApiDbEntities1 _dbEntities;
        private HttpClient _client;

        private HttpResponseMessage _response;
        private string _token;
        private const string ServiceBaseURL = "http://localhost:64809";

        #region SetUp
        [OneTimeSetUp]
        public void Setup()
        {
            _products = SetUpProducts();
            _tokens = SetUpTokens();
            _dbEntities = new Mock<WebApiDbEntities1>().Object;
            _tokenRepository = SetUpTokenRepository();
            _productRepository = SetUpProductRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(s => s.ProductRepository).Returns(_productRepository);
            unitOfWork.SetupGet(s => s.TokenRepository).Returns(_tokenRepository);
            _unitOfWork = unitOfWork.Object;
            _productService = new ProductServices(_unitOfWork);
            _tokenService = new TokenServices(_unitOfWork);
            _client = new HttpClient { BaseAddress = new Uri(ServiceBaseURL) };
            var tokenEntity = _tokenService.GenerateToken(1);
            _token = tokenEntity.AuthToken;
            _client.DefaultRequestHeaders.Add("Token", _token);
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _client = new HttpClient { BaseAddress = new Uri(ServiceBaseURL) };
            _client.DefaultRequestHeaders.Add("Token", _token);
        }

        private static List<Product> SetUpProducts()
        {
            var prodId = new int();
            var products = DataInitializer.GetAllProducts();
            foreach (Product prod in products)
                prod.ProductId = ++prodId;
            return products;
        }

        private static List<Token> SetUpTokens()
        {
            var tokId = new int();
            var tokens = DataInitializer.GetAllTokens();
            foreach (Token tok in tokens)
                tok.TokenId = ++tokId;
            return tokens;
        }

        private GenericRepository<Product> SetUpProductRepository()
        {
            // Initialise repo
            var mockRepo = new Mock<GenericRepository<Product>>(MockBehavior.Default, _dbEntities);

            // Setup mocking behavior
            mockRepo.Setup(p => p.GetAll()).Returns(_products);

            mockRepo.Setup(p => p.GetByID(It.IsAny<int>()))
                .Returns(new Func<int, Product>(id => _products.Find(p => p.ProductId.Equals(id))));

            mockRepo.Setup(p => p.Insert((It.IsAny<Product>())))
                .Callback(new Action<Product>(newProduct =>
                {
                    dynamic maxProductID = _products.Last().ProductId;
                    dynamic nextProductID = maxProductID + 1;
                    newProduct.ProductId = nextProductID;
                    _products.Add(newProduct);
                }));

            mockRepo.Setup(p => p.Update(It.IsAny<Product>()))
                .Callback(new Action<Product>(prod =>
                {
                    var oldProduct = _products.Find(a => a.ProductId == prod.ProductId);
                    oldProduct = prod;
                }));

            mockRepo.Setup(p => p.Delete(It.IsAny<Product>()))
                .Callback(new Action<Product>(prod =>
                {
                    var productToRemove = _products.Find(a => a.ProductId == prod.ProductId);

                    if (productToRemove != null)
                        _products.Remove(productToRemove);
                }));

            return mockRepo.Object;
        }

        private GenericRepository<Token> SetUpTokenRepository()
        {
            // Initialise repo
            var mockRepo = new Mock<GenericRepository<Token>>(MockBehavior.Default, _dbEntities);

            // Setup mocking behavior
            mockRepo.Setup(p => p.GetAll()).Returns(_tokens);

            mockRepo.Setup(p => p.GetByID(It.IsAny<int>()))
                .Returns(new Func<int, Token>(id => _tokens.Find(p => p.TokenId.Equals(id))));

            mockRepo.Setup(p => p.Insert((It.IsAny<Token>())))
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
        #endregion

        #region Tests
        [Test]
        public void GetAllProductsTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ServiceBaseURL + "/api/product")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            _response = productController.Get();

            var responseResult = JsonConvert.DeserializeObject<List<Product>>(_response.Content.ReadAsStringAsync().Result);
            Assert.AreEqual(_response.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(responseResult.Any(), true);
            var comparer = new ProductComparer();
            CollectionAssert.AreEqual(
            responseResult.OrderBy(product => product, comparer),
            _products.OrderBy(product => product, comparer), comparer);
        }
        #endregion

        #region TearDown
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _tokenService = null;
            _productService = null;
            _unitOfWork = null;
            _tokenRepository = null;
            _productRepository = null;
            _tokens = null;
            _products = null;
            if (_response != null)
                _response.Dispose();
            if (_client != null)
                _client.Dispose();
        }

        [TearDown]
        public void DisposeTest()
        {
            if (_response != null)
                _response.Dispose();
            if (_client != null)
                _client.Dispose();
        }
        #endregion
    }
}
