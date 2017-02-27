using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModel.GenericRepository;

namespace DataModel.UnitOfWork
{
    public interface IUnitOfWork
    {
        GenericRepository<Product> ProductRepository { get;  }
        GenericRepository<User> UserRepository { get;  }
        GenericRepository<Token> TokenRepository { get; }
        /// <summary>
        /// Save method.
        /// </summary>
        void Save();
    }
}
