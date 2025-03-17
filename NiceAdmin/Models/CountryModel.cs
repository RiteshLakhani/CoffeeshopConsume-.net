using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace NiceAdmin.Models
{
    public class CountryModel
    {
        [Key]
        public int? CountryID { get; set; }

        [Required]
        [DisplayName("Country Name")]
        public string CountryName { get; set; }

        [Required]
        [DisplayName("Country Code")]
        public string CountryCode { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; } 

        [DisplayName("Modified Date")]
        public DateTime? ModifiedDate { get; set; } 
    }


}
