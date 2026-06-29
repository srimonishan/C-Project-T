using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalArtisanCraftMarket.Models
{
    internal class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int CraftItemID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        // Extra fields (not in DB, just for display in the form)
        public string ItemName { get; set; }
    }
}
