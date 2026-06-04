using CosmeticsStore.Controllers;
using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CosmeticsStore.Tests
{
    [TestClass]
    public class ProductsControllerTests
    {
        private Mock<ApplicationDbContext> _mockContext;
        private Mock<DbSet<Product>> _mockProductSet;
        private ProductsController _controller;
        private List<Product> _products;

        [TestInitialize]
        public void Setup()
        {
            _products = new List<Product>
            {
                new Product { Id = 1, Title = "Lipstick A", Alias = "lipstick-a", Price = 100, IsActive = true, IsHome = true },
                new Product { Id = 2, Title = "Lipstick B", Alias = "lipstick-b", Price = 200, IsActive = true, IsHome = true },
                new Product { Id = 3, Title = "Foundation", Alias = "foundation", Price = 150, IsActive = true, IsHome = false }
            };

            var data = _products.AsQueryable();

            _mockProductSet = new Mock<DbSet<Product>>();
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(data.Provider);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(data.Expression);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(data.ElementType);
            _mockProductSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            _mockContext = new Mock<ApplicationDbContext>();
            _mockContext.Setup(c => c.Products).Returns(_mockProductSet.Object);

            _controller = new ProductsController(_mockContext.Object);
        }

        [TestMethod]
        public void Index_ReturnsViewWithProducts()
        {
            // Act
            var result = _controller.Index(null, 1) as ViewResult;
            var model = result.Model as IEnumerable<Product>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(3, model.Count());
        }

        [TestMethod]
        public void Index_SearchWithKeyword_ReturnsFilteredProducts()
        {
            // Act
            var result = _controller.Index("Foundation", 1) as ViewResult;
            var model = result.Model as IEnumerable<Product>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, model.Count());
            Assert.AreEqual("Foundation", model.First().Title);
        }

        [TestMethod]
        public void SortByName_ReturnsSortedAscending()
        {
            // Act
            var result = _controller.SortByName(null) as ViewResult;
            var model = result.Model as IEnumerable<Product>;

            // Assert
            var productList = model.ToList();
            Assert.AreEqual("Foundation", productList[0].Title);
            Assert.AreEqual("Lipstick A", productList[1].Title);
            Assert.AreEqual("Lipstick B", productList[2].Title);
        }

        [TestMethod]
        public void Partial_ItemsByCateId_ReturnsOnlyIsHomeAndActive()
        {
            // Act
            var result = _controller.Partial_ItemsByCateId() as PartialViewResult;
            var model = result.Model as IEnumerable<Product>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, model.Count());
            Assert.IsTrue(model.All(p => p.IsHome && p.IsActive));
        }
    }
}
