using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaraKutuTestOrnek2
{
    public class ShoppingService
    {

        public readonly AppDbContext dbContext;

        public ShoppingService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public void AddUser(string fullName, bool isPremium)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException("FullName Boş Geçilemez");

            dbContext.Users.Add(new User() {FullName=fullName, IsPremiumUser = isPremium });
            dbContext.SaveChanges();
        }


        public void AddOrder(int userId, decimal totalAmount)
        {
            if (userId < 0)
                throw new InvalidDataException("UserId değeri negatif olamaz");
            if (totalAmount < 0) throw new InvalidDataException("Bir Siparişin Toplam Miktarı Negatif Olamaz");


            User? user = dbContext.Users.FirstOrDefault(u=>u.Id == userId);
            if (user == null)
                throw new ArgumentNullException("İlgili User Bulunamadı");


            dbContext.Orders.Add(new Order() { UserId = userId, Total=totalAmount, Status=OrderStatus.Created});
            dbContext.SaveChanges();
        }

        //İlgili sipariş'in indirim uygulanabilir olup olmadığını kontrol et.
        public bool IsOrderEligibleForDiscount(int orderId)
        {
            Order? order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                throw new ArgumentNullException("İlgili Sipariş Bulunamadı");

            User? user = dbContext.Users.FirstOrDefault(u => u.Id == order.UserId);

            //Eğer kullanıcı premium user ise siparişin tutarına bakılmaksızın true döneriz
            if (user.IsPremiumUser)
                return true;

            //Eğer kullanıcı standart bir kullancıysa siparişin tutarına bakılır. Sipariş tutarı 100'den büyükse true değilse false döneriz.
            return order.Total >= 100;
        }

        //Bir siparişin durumunu güncellemek için kullanacağız.
        public bool TransitionOrderStatus(int orderId, OrderStatus newOrderStatus)
        {
            if(orderId < 0) throw new InvalidDataException("OrderId negatif olamaz");

            Order? order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                throw new ArgumentNullException("Order Bulunamadı");


            //Bir siparişteki geçerli durum hangi durumlara geçiş yapabilir?
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                {OrderStatus.Created, new List<OrderStatus>{OrderStatus.Processing} },
                {OrderStatus.Processing, new List<OrderStatus>{OrderStatus.Completed} },
                {OrderStatus.Completed , new List<OrderStatus>{}}
            };


            //eğer sipariş, parametre olarak gelen newStatus'e geçiş yapamıyorsa false döner
            if (!validTransitions[order.Status].Contains(newOrderStatus))
                return false;

            //eğer sipariş, parametre olarak gelen newStatus'e geçiş yapabiliyorsa bu şekilde güncelle.
            order.Status = newOrderStatus;
            dbContext.SaveChanges();
            return true;
        }
    }
}
