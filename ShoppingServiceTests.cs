using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace KaraKutuTestOrnek2
{
    public class ShoppingServiceTests
    {
        private readonly ITestOutputHelper output;
        public ShoppingServiceTests(ITestOutputHelper output)
        {
            this.output = output;
            InitializeShoppingService();
        }
        private  ShoppingService shoppingService;

        private void InitializeShoppingService()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
            var appDbContext = new AppDbContext(options);
            
            shoppingService = new ShoppingService(appDbContext);
            
            seedDatabase(); //shoppingService'in veritabanına 2 tane kullanıcı ekleyecek.

        }

        public void seedDatabase()
        {
            shoppingService.AddUser("Ahmet Standart Kullanıcı",false);// Id=1 olacak
            shoppingService.AddUser("Mehmet Premium Kullanıcı",true); // Id=2 Olacak.
        }


        [Theory]
        [InlineData(1, 50, false)]  // Standart kullanıcı, düşük totalAmount
        [InlineData(1, 150, true)] // Standart kullanıcı, yüksek totalAmount
        [InlineData(2, 50, true)]  // Premium kullanıcı, düşük totalAmount
        public void IsOrderEligibleForDiscount_ShouldReturnCorrectResult(int userId, decimal totalAmount, bool expected)
        {

            shoppingService.AddOrder(userId, totalAmount);

            int orderId = shoppingService.dbContext.Orders.Last().Id;

            bool result = shoppingService.IsOrderEligibleForDiscount(orderId);

            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(1, 99.99, false)] // Sınırın hemen altında
        [InlineData(1, 100.00, true)] // Tam sınırda
        [InlineData(1, 100.01, true)] // Sınırın hemen üstünde
        public void IsOrderEligibleForDiscount_ShouldReturnCorrectResultAtBoundaries(int userId, decimal totalAmount, bool expected)
        {
            shoppingService.AddOrder(userId, totalAmount);

            int orderId = shoppingService.dbContext.Orders.Last().Id;

            bool result = shoppingService.IsOrderEligibleForDiscount(orderId);

            Assert.Equal(expected, result);

        }

        [Theory]
        [InlineData(false, 50, false)] // Standart kullanıcı, düşük toplam
        [InlineData(false, 150, true)] // Standart kullanıcı, yüksek toplam
        [InlineData(true, 50, true)]  // Premium kullanıcı, düşük toplam
        [InlineData(true, 150, true)] // Premium kullanıcı, yüksek toplam
        public void IsOrderEligibleForDiscount_ShouldMatchDecisionTable(bool isPremiumUser, decimal totalAmount, bool expected)
        {
            User? user = shoppingService.dbContext.Users.FirstOrDefault(u => u.IsPremiumUser == isPremiumUser);

            if (user == null) throw new ArgumentException("Error!");

            shoppingService.AddOrder(user.Id, totalAmount);

            int orderId = shoppingService.dbContext.Orders.Last().Id;

            bool result = shoppingService.IsOrderEligibleForDiscount(orderId);

            Assert.Equal(expected, result);
        }

        [Fact]
        
        public void TransitionOrderStatus_ShouldHandleValidAndInvalidTransitions()
        {
            shoppingService.AddOrder(1, 150);
            int orderId = shoppingService.dbContext.Orders.Last().Id;

            bool result1 = shoppingService.TransitionOrderStatus(orderId, OrderStatus.Completed); //Expected false
            bool result2 = shoppingService.TransitionOrderStatus(orderId, OrderStatus.Processing); //Expected True
            bool result3 = shoppingService.TransitionOrderStatus(orderId, OrderStatus.Completed); //Expected True
            bool result4 = shoppingService.TransitionOrderStatus(orderId, OrderStatus.Completed); //Expected false

            Assert.False(result1);
            Assert.True(result2);
            Assert.True(result3);
            Assert.False(result4);


        }
    }
}
