using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel;
using DataModel.UnitOfWork;
using DataModel.GenericRepository;
using NUnit.Framework;
using TestHelper;
using Moq;
using AutoMapper;
using BusinessEntities;

namespace BusinessServices.Tests
{
    public class ProductServicesTests
    {
        private IProductServices _productService;
        private IUnitOfWork _unitOfWork;
        private List<Product> _products;
        private GenericRepository<Product> _productRepository;
        private WebApiDbEntities1 _dbEntities;

        #region Test fixture OneTimeSetup
        /// <summary>
        /// Initial setup for tests
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            _products = SetUpProducts();
        }

        private static List<Product> SetUpProducts()
        {
            var prodId = new int();
            var products = DataInitializer.GetAllProducts();
            foreach (Product prod in products)
                prod.ProductId = ++prodId;
            return products;
        }
        #endregion

        #region Setup
        /// <summary>
        /// Re-initialzes test
        /// </summary>
        [SetUp]
        public void ReIntializeTest()
        {
            _dbEntities = new Mock<WebApiDbEntities1>().Object;
            _productRepository = SetUpProductRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(s => s.ProductRepository).Returns(_productRepository);
            _unitOfWork = unitOfWork.Object;
            _productService = new ProductServices(_unitOfWork);
        }

        private GenericRepository<Product> SetUpProductRepository()
        {
            // Initialise repo
            var mockRepo = new Mock<GenericRepository<Product>>(MockBehavior.Default, _dbEntities);

            // setup mocking behavior
            mockRepo.Setup(p => p.GetAll()).Returns(_products);

            mockRepo.Setup(p => p.GetByID(It.IsAny<int>()))
                .Returns(new Func<int, Product>(
                    id => _products.Find(p => p.ProductId.Equals(id))));

            mockRepo.Setup(p => p.Insert(It.IsAny<Product>()))
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
                    {
                        _products.Remove(productToRemove);
                    }
                }));

            return mockRepo.Object;
        }
        #endregion

        #region Tests
        [Test]
        public void GetAllProductsTest()
        {
            var products = _productService.GetAllProducts();
            var productList = products.Select(
                productEntity => new Product
                {
                    ProductId = productEntity.ProductId,
                    ProductName = productEntity.ProductName
                }).ToList();
            var comparer = new ProductComparer();
            CollectionAssert.AreEqual(
                productList.OrderBy(product => product, comparer),
                _products.OrderBy(product => product, comparer), comparer);
        }

        [Test]
        public void GetAllProductsTestForNull()
        {
            _products.Clear();
            var products = _productService.GetAllProducts();
            Assert.Null(products);
            SetUpProducts();
        }

        [Test]
        public void GetProductByRightIdTest()
        {
            var mobileProduct = _productService.GetProductById(2);
            if (mobileProduct != null)
            {
                var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<ProductEntity, Product>());

                var mapper = config.CreateMapper();
                var productModel = mapper.Map<ProductEntity, Product>(mobileProduct);
                AssertObjects.PropertyValuesAreEquals(productModel, _products.Find(a => a.ProductName.Contains("Mobile")));
            }
        }

        [Test]
        public void GetProductByWrongIdTest()
        {
            var product = _productService.GetProductById(0);
            Assert.Null(product);
        }

        [Test]
        public void AddNewProductTest()
        {
            var newProduct = new ProductEntity()
            {
                ProductName = "Android Phone"
            };
            var maxProductIDBeforeAdd = _products.Max(a => a.ProductId);
            newProduct.ProductId = maxProductIDBeforeAdd + 1;
            _productService.CreateProduct(newProduct);
            var addedproduct = new Product()
            {
                ProductName = newProduct.ProductName,
                ProductId = newProduct.ProductId
            };
            AssertObjects.PropertyValuesAreEquals(addedproduct, _products.Last());
            Assert.That(maxProductIDBeforeAdd + 1, Is.EqualTo(_products.Last().ProductId));
        }

        [Test]
        public void UpdateProductTest()
        {
            var firstProduct = _products.FirstOrDefault();
            firstProduct.ProductName = "Laptop updated";
            var updatedProduct = new ProductEntity() { ProductName = firstProduct.ProductName, ProductId = firstProduct.ProductId };
            _productService.UpdateProduct(firstProduct.ProductId, updatedProduct);
            Assert.That(firstProduct.ProductId, Is.EqualTo(1)); // hasn't changed
            Assert.That(firstProduct.ProductName, Is.EqualTo("Laptop updated")); // Product name changed
        }

        [Test]
        public void DeleteProductTest()
        {
            int maxID = _products.Max(a => a.ProductId);
            var lastProduct = _products.Last();
            _productService.DeleteProduct(lastProduct.ProductId);
            Assert.That(maxID, Is.GreaterThan(_products.Max(a => a.ProductId)));
        }
        #endregion

        #region TestFixture OneTimeTearDown
        /// <summary>
        /// Tears down each test data
        /// </summary>
        [TearDown]
        public void DisposeTest()
        {
            _productService = null;
            _unitOfWork = null;
            _productRepository = null;
            if (_dbEntities != null) _dbEntities.Dispose();
        }

        /// <summary>
        /// TestFixture teardown
        /// </summary>
        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _products = null;
        }
        #endregion
    }
}
