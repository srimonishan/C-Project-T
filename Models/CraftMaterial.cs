using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalArtisanCraftMarket.Models
{
    internal class CraftMaterial
    {
        public int CraftMaterialID { get; set; }
        public int CraftItemID { get; set; }
        public string ItemName { get; set; }   // from CraftItems
        public int MaterialID { get; set; }
        public string MaterialName { get; set; } // from Materials
        public string Unit { get; set; }
        public decimal QuantityUsed { get; set; }
        public decimal Cost { get; set; }
    }
}
