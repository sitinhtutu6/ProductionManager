using ProductionManager.Data;
using System.Collections.Generic;

namespace ProductionApp.Web.Models
{
    public class ProductIndexViewModel
    {
        // Chứa danh sách sản phẩm
        public List<Product> Products { get; set; } = new List<Product>();

        // Chứa danh sách vật tư
        public List<Material> Materials { get; set; } = new List<Material>();
    }
}