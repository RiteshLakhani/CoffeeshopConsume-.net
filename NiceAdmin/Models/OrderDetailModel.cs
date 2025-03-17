using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class OrderDetailModel
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "Order ID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        [StringLength(50, ErrorMessage = "Product ID cannot be longer than 50 characters")]
        public string ProductID { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal? Amount { get; set; }

        [Required]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserID { get; set; }
    }
}
