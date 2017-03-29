using AutoMapper;
using BusinessEntities;
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
    public class ProductServicesTests
    {
        private IProductServices _productService;
        private IUnitOfWork _unitOfWork;
        private List<Product> _products;
        private GenericRepository<Product> _productRepository;
        private WebApiDbEntities _dbEntities;

        public ProductServicesTests()
        {
            _products = SetUpProducts();
            _dbEntities = new Mock<WebApiDbEntities>().Object;
            _productRepository = SetUpProductRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(s => s.ProductRepository).Returns(_productRepository);
            _unitOfWork = unitOfWork.Object;
            _productService = new ProductServices(_unitOfWork);
        }

        public void Dispose()
        {
            _products = null;
        }

        private static List<Product> SetUpProducts()
        {
            var prodId = new int();
            var products = DataInitializer.GetAllProducts();
            foreach (Product prod in products)
                prod.ProductId = ++prodId;
            return products;
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

        /// <summary>
        /// Service should return all the products
        /// </summary>
        [Fact]
        public void GetAllProductsTest()
        {
            var products = _productService.GetAllProducts();
            var productList = products.Select(productEntity => new Product { ProductId = productEntity.ProductId, ProductName = productEntity.ProductName }).ToList();
            var comparer = new ProductComparer();
            Assert.Equal(productList.Count, _products.Count);
        }

        /// <summary>
        /// Service should return null
        /// </summary>
        [Fact]
        public void GetAllProductsTestForNull()
        {
            _products.Clear();
            var products = _productService.GetAllProducts();
            Assert.Null(products);
            SetUpProducts();
        }

        /// <summary>
        /// Service should return product if correct id is supplied
        /// </summary>
        [Fact]
        public void GetProductByRightIdTest()
        {
            var mobileProduct = _productService.GetProductById(2);
            if (mobileProduct != null)
            {
                var config = new MapperConfiguration(cfg =>
                    cfg.CreateMap<ProductEntity, Product>());

                var mapper = config.CreateMapper();
                Product productModel = mapper.Map<Product>(mobileProduct);
                var comparer = new ProductComparer();
                Assert.Equal(productModel.ToString(), _products.Find(a => a.ProductName.Contains("Mobile")).ToString());
            }
        }

        /// <summary>
        /// Service should return null
        /// </summary>
        [Fact]
        public void GetProductByWrongIdTest()
        {
            var product = _productService.GetProductById(0);
            Assert.Null(product);
        }

        /// <summary>
        /// Add new product test
        /// </summary>
        [Fact]
        public void AddNewProductTest()
        {
            var newProduct = new ProductEntity()
            {
                ProductName = "Android Phone"
            };

            var maxProductIDBeforeAdd = _products.Max(a => a.ProductId);
            newProduct.ProductId = maxProductIDBeforeAdd + 1;
            _productService.CreateProduct(newProduct);
            var addedproduct = new Product() { ProductName = newProduct.ProductName, ProductId = newProduct.ProductId };
            Assert.True((maxProductIDBeforeAdd + 1).Equals(_products.Last().ProductId));
        }

        /// <summary>
        /// Update product test
        /// </summary>
        [Fact]
        public void UpdateProductTest()
        {
            var firstProduct = _products.First();
            firstProduct.ProductName = "Laptop updated";
            var updatedProduct = new ProductEntity()
            {
                ProductName = firstProduct.ProductName,
                ProductId = firstProduct.ProductId
            };
            _productService.UpdateProduct(firstProduct.ProductId, updatedProduct);
            Assert.True(firstProduct.ProductId.Equals(1));
            Assert.True(firstProduct.ProductName.Equals("Laptop updated"));
        }

        /// <summary>
        /// Delete product test
        /// </summary>
        [Fact]
        public void DeleteProductTest()
        {
            int maxID = _products.Max(a => a.ProductId);
            var lastProduct = _products.Last();

            _productService.DeleteProduct(lastProduct.ProductId);
            Assert.True(maxID > (_products.Max(a => a.ProductId)));
        }
    }
}