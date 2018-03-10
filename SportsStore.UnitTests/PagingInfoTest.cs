using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsStore.WebUi.HtmlHelpers;
using System.Net.Http;
using SportsStore.WebUi.Models;
using System.Web.Mvc;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using System.Linq;
using SportsStore.WebUi.Controllers;
using System.Collections.Generic;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class PagingInfoTest
    {
        [TestMethod]
        public void Can_Paginate()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product { ProductId = 1, Name = "P1" },
                new Product { ProductId = 2, Name = "P2" },
                new Product { ProductId = 3, Name = "P3" },
                new Product { ProductId = 4, Name = "P4" },
                new Product { ProductId = 5, Name = "P5" }
            }.AsQueryable());

            //Arrange
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Action
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            //Assert
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            //Arrange (установки) опрделяем html helper - нам нужно это для применения метода расширения.
            HtmlHelper myHelper = null;
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            Func<int, string> pageUrlDelegate = i => "Page" + i;
            //Act
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);

            //Assert
            Assert.AreEqual(result.ToString(), @"<a href=""Page1"">1</a>" +
                @"<a class=""selected"" href=""Page2"">2</a>" +
                @"<a href=""Page3"">3</a>");
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]{
                new Product { ProductId = 1, Name = "P1" },
                new Product { ProductId = 2, Name = "P2" },
                new Product { ProductId = 3, Name = "P3" },
                new Product { ProductId = 4, Name = "P4" },
                new Product { ProductId = 5, Name = "P5" }
            }.AsQueryable());

            //Arrange
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Act
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            //Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);

        }

        [TestMethod]
        public void Can_Filter_Product()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product { ProductId = 1, Name = "P1", Category = "Cat1" },
                new Product { ProductId = 2, Name = "P2", Category = "Cat1" },
                new Product { ProductId = 3, Name = "P3", Category = "Cat2" },
                new Product { ProductId = 4, Name = "P4", Category = "Cat1" },
                new Product { ProductId = 5, Name = "P5", Category = "Cat3" }
            }.AsQueryable());

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Action
            Product[] result = ((ProductsListViewModel)controller.List("Cat2", 1).Model).Products.ToArray();

            //Assert
            Assert.AreEqual(result.Length, 1);
            Assert.IsTrue(result[0].Name == "P3" && result[0].Category == "Cat2");
        }

        [TestMethod]
        public void Can_Create_Categories()
        {
            //Arrange
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product { ProductId = 1, Name = "P1", Category = "Apples" },
                new Product { ProductId = 2, Name = "P2", Category = "Oranges" },
                new Product { ProductId = 3, Name = "P3", Category = "Apples" },
                new Product { ProductId = 4, Name = "P4", Category = "Cocos" },
                new Product { ProductId = 5, Name = "P5", Category = "Apples" },
            }.AsQueryable());
            //Arrange
            NavController target = new NavController(mock.Object);

            //Act
            string[] result = ((IEnumerable<string>)target.Menu().Model).ToArray();

            //Assert
            Assert.AreEqual(result.Length, 3);
            Assert.AreEqual(result[0], "Apples");
            Assert.AreEqual(result[1], "Cocos");
            Assert.AreEqual(result[2], "Oranges");
        }
        [TestMethod]
        public void Generate_Category_Specific_Product_Count()
        {
            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductId = 1, Name = "P1", Category = "Cat1" },
                new Product {ProductId = 2, Name = "P2", Category = "Cat1" },
                new Product {ProductId = 3, Name = "P3", Category = "Cat3" },
                new Product {ProductId = 4, Name = "P4", Category = "Cat1" },
                new Product {ProductId = 5, Name = "P5", Category = "Cat2" },
                new Product {ProductId = 6, Name = "P6", Category = "Cat2" }
            }.AsQueryable());
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            int res1 = ((ProductsListViewModel)controller.List("Cat1").Model).PagingInfo.TotalItems;
            int res2 = ((ProductsListViewModel)controller.List("Cat2").Model).PagingInfo.TotalItems;
            int res3 = ((ProductsListViewModel)controller.List("Cat3").Model).PagingInfo.TotalItems;

            Assert.AreEqual(res1, 3);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
        }
    }
}
