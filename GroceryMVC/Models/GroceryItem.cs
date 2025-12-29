using System.ComponentModel.DataAnnotations;

namespace GroceryMVC.Models
{
    public class GroceryItem
    {
        [Key]
        public int GroceryId { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
