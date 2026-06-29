using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalArtisanCraftMarket.Models
{
    internal class Material
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal UnitCost { get; set; }
        public decimal AvailableQuantity { get; set; }
        public string Supplier { get; set; }
        public DateTime LastUpdated { get; set; }

        public override string ToString()
        {
            return MaterialName;
        }
    }
}
