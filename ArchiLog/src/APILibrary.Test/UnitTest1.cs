using APILibrary.Test.Mock;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace APILibrary.Test
{
    public class Tests
    {
        private MockDbContext _db;
        private PizzasController _controllerP;
        private CustomersController _controllerC;
        [SetUp]
        public void Setup()
        {
            _db = MockDbContext.GetDbContext();
            _controllerP = new PizzasController(_db);
            _controllerC = new CustomersController(_db);
        }

        [Test]
        public async Task Test1()
        {
            var actionResult = await _controllerP.GetAllAsync("", "", "","","","","");
            var result = actionResult.Result as ObjectResult;
            var values = ((IEnumerable<object>)(result).Value);

            Assert.AreEqual((int)System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(_db.Pizzas.Count(), values.Count());
        }
        [Test]
        public async Task Test2()
        {
            var actionResult = await _controllerC.SearchAsync("test");
            var result = actionResult.Result as ObjectResult;
            var values = (IEnumerable<object>)(result).Value;
            Assert.AreEqual((int)System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(_db.Customers.Count(), values.Count());
        }
    }
}