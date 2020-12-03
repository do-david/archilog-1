using APILibrary.Test.Mock.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebApplication.Data;

namespace APILibrary.Test.Mock
{
    public class MockDbContext : EatDbContext
    {
        public MockDbContext(DbContextOptions options) : base(options)
        {
        }

        public static MockDbContext GetDbContext(bool withData = true)
        {
            var options = new DbContextOptionsBuilder().UseInMemoryDatabase("dbtest").Options;
            var db = new MockDbContext(options);

            if (withData)
            {
                db.Pizzas.Add(new PizzaMock { Name = "Pizza 1", Price = 10, Topping = "Champignon" });
                db.Pizzas.Add(new PizzaMock { Name = "Pizza 2", Price = 11, Topping = "Champignon" });
                db.Pizzas.Add(new PizzaMock { Name = "Pizza 3", Price = 12, Topping = "Champignon" });
                db.Customers.Add(new CustomerMock { Email ="test1@test.com", Phone ="0123455678", Lastname="Test1"});
                db.Customers.Add(new CustomerMock { Email = "test2@test.com", Phone = "0123455679", Lastname = "Test2" });
                db.Customers.Add(new CustomerMock { Email = "test3@test.com", Phone = "0123455670", Lastname = "Test3" });

                db.SaveChanges();
            }

            return db;
        }
    }
}
