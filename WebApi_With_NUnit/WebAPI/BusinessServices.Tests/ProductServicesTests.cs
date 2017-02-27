using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.UnitOfWork;
using DataModel;

namespace BusinessServices.Tests
{
    public class ProductServicesTests
    {
        private IProductServices _productService;
        private IUnitOfWork _unitOfWork;
        private List<Product> _productRepository;
        private WebApiDbEntities1 _dbEntities;
    }
}
