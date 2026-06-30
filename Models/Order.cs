using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalArtisanCraftMarket.Models
{
    internal class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }   // "Placed","Packed","Shipped","Delivered","Cancelled"
        public string DeliveryAddress { get; set; }
        public string ContactNumber { get; set; }
    }
}
