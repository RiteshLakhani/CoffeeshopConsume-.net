using System;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class OrderModel
    {
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage ="OrderCode is required")]
        public string OrderCode { get; set; }

        [Required(ErrorMessage = "Order Date is required")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Payment Mode is required")]
        [StringLength(50, ErrorMessage = "Payment Mode cannot be longer than 50 characters")]
        public string PaymentMode { get; set; }

        [Required(ErrorMessage ="Total Amount is Required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Amount must be a positive number")]
        public decimal? TotalAmount { get; set; }

        [Required(ErrorMessage = "Shipping Address is required")]
        [StringLength(200, ErrorMessage = "Shipping Address cannot be longer than 200 characters")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserID { get; set; }
    }

    public class OrderDropDownModel
    {
        public int OrderID { get; set; }
        public string OrderCode { get; set; }
    }
}
