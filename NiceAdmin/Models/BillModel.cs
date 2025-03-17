using System;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class BillModel
    {
        [Key]
        public int BillID { get; set; }

        [Required(ErrorMessage = "Bill Number is required")]
        [StringLength(50, ErrorMessage = "Bill Number cannot be longer than 50 characters")]
        public string BillNumber { get; set; }

        [Required(ErrorMessage = "Bill Date is required")]
        [DataType(DataType.DateTime)]
        public DateTime BillDate { get; set; }

        [Required(ErrorMessage = "Order ID is required")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Total Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Amount must be a positive number")]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage ="Discount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount must be a positive number")]
        public decimal? Discount { get; set; }

        [Required(ErrorMessage = "Net Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Net Amount must be a positive number")]
        public decimal? NetAmount { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserID { get; set; }
    }
}
