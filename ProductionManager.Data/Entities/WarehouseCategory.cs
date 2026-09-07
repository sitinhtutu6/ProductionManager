using System.ComponentModel.DataAnnotations;

namespace ProductionManager.Data
{
    public class WarehouseCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // Quan hệ 1-Nhiều với vật tư
        public ICollection<WarehouseItem> WarehouseItems { get; set; }
    }
}