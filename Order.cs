using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaraKutuTestOrnek2
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Total {  get; set; }
        public OrderStatus Status { get; set; }
        
    }

    
}
