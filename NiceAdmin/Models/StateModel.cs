using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class StateModel
    {
        public int? StateID { get; set; }

        [Required(ErrorMessage = "State Name is required")]
        public string StateName { get; set; }

        [Required(ErrorMessage = "State Code is required")]
        public string StateCode { get; set; }

        [Required(ErrorMessage = "Country ID is required")]
        public int CountryID { get; set; }
    }
}
