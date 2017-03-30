using BusinessServices;
using DataModel;
using DataModel.GenericRepository;
using DataModel.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using WebApi.Controllers;
using Xunit;
using System.Web.Http.Hosting;
using Newtonsoft.Json;
using System.Net;
using BusinessEntities;

namespace ApiController.Tests
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
        private WebApiDbEntities _dbEntities;
        private HttpClient _client;

        private HttpResponseMessage _response;
        private string _token;
        private const string ServiceBaseURL = "http://localhost:52048/";

        public ProductControllerTest()
        {
            _products = SetUpProducts();
            _tokens = SetUpTokens();
            _dbEntities = new Mock<WebApiDbEntities>().Object;
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

        public void Dispose()
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
            var mockRepo = new Mock<GenericRepository<Product>>(MockBehavior.Default, _dbEntities);

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
            var mockRepo = new Mock<GenericRepository<Token>>(MockBehavior.Default, _dbEntities);

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

        [Fact]
        public void GetAllProductsTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ServiceBaseURL + "api/product")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            _response = productController.Get();

            var responseResult = JsonConvert.DeserializeObject<List<Product>>(_response.Content.ReadAsStringAsync().Result);
            Assert.Equal(_response.StatusCode, HttpStatusCode.OK);
            Assert.True(responseResult.Any());
            Assert.Equal(responseResult.Count, _products.Count);
        }

        [Fact]
        public void GetProductByIdTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/1")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            _response = productController.Get(2);

            var responseResult = JsonConvert.DeserializeObject<Product>(_response.Content.ReadAsStringAsync().Result);
            Assert.Equal(_response.StatusCode, HttpStatusCode.OK);
            Assert.True(responseResult.ProductName.Equals("Mobile"));
        }

        [Fact]
        public void GetProductByWrongIdTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/10")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            _response = productController.Get(10);
            Assert.Equal(_response.StatusCode, HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetProductByInvalidIdTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/-1")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            _response = productController.Get(-1);
            Assert.Equal(_response.StatusCode, HttpStatusCode.NotFound);
        }

        [Fact]
        public void CreateProductTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var newProduct = new ProductEntity()
            {
                ProductName = "Android Phone"
            };

            var maxProductIDBeforeAdd = _products.Max(a => a.ProductId);
            newProduct.ProductId = maxProductIDBeforeAdd + 1;
            productController.Post(newProduct);
            var addedProduct = new Product()
            {
                ProductName = newProduct.ProductName,
                ProductId = newProduct.ProductId
            };
            Assert.Equal(maxProductIDBeforeAdd + 1, (_products.Last().ProductId));
        }

        [Fact]
        public void UpdateProductTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/1")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var firstProduct = _products.First();
            firstProduct.ProductName = "Laptop updated";
            var updatedProduct = new ProductEntity()
            {
                ProductName = firstProduct.ProductName,
                ProductId = firstProduct.ProductId
            };
            productController.Put(firstProduct.ProductId, updatedProduct);
            Assert.Equal(firstProduct.ProductId, 1);
        }

        [Fact]
        public void DeleteProductTest()
        {
            var productController = new ProductController(_productService)
            {
                Request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri(ServiceBaseURL + "api/product/1")
                }
            };
            productController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            int maxID = _products.Max(a => a.ProductId);
            var lastProduct = _products.Last();

            productController.Delete(lastProduct.ProductId);
            Assert.True(maxID > (_products.Max(a => a.ProductId)));
        }
    }
}
