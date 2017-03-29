using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHelper
{
    public class DataInitializer
    {
        /// <summary>
        /// Dummy Products
        /// </summary>
        /// <returns></returns>
        public static List<Product> GetAllProducts()
        {
            var products = new List<Product>
            {
                new Product() {ProductName = "Laptop" },
                new Product() {ProductName = "Mobile" },
                new Product() {ProductName = "HardDrive" },
                new Product() {ProductName = "IPhone" },
                new Product() {ProductName = "IPad" }
            };
            return products;
        }

        ///<summary>
        /// Dummy Tokens
        /// </summary>
        public static List<Token> GetAllTokens()
        {
            var tokens = new List<Token>
            {
                new Token()
                {
                    AuthToken = "9f907bdf-f6de-425d-be5b-b4852eb77761",
                    ExpiresOn = DateTime.Now.AddHours(2),
                    IssuedOn = DateTime.Now,
                    UserId = 1
                },
                new Token()
                {
                    AuthToken = "9f907bdf-f6de-425d-be5b-b4852eb77762",
                    ExpiresOn = DateTime.Now.AddHours(2),
                    IssuedOn = DateTime.Now,
                    UserId = 2
                },
            };
            return tokens;
        }

        public static List<User> GetAllUsers()
        {
            var users = new List<User>
            {
                new User()
                {
                    UserName = "akhil",
                    Password = "akhil",
                    Name = "Akhil"
                },
                new User()
                {
                    UserName = "arsh",
                    Password = "arsh",
                    Name = "Arsh"
                },
                new User()
                {
                    UserName = "divit",
                    Password = "divit",
                    Name = "Divit"
                }
            };
            return users;
        }
    }
}
