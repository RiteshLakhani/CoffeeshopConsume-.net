using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class ProdcutModel
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, ErrorMessage = "Product Name cannot exceed 100 characters")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product Price is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Product Price must be a positive number")]
        public double? ProductPrice { get; set; }

        [Required(ErrorMessage = "Product Code is required")]
        [StringLength(50, ErrorMessage = "Product Code cannot exceed 50 characters")]
        public string ProductCode { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserID { get; set; }
    }

    public class ProductDropDownModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
    }
}
