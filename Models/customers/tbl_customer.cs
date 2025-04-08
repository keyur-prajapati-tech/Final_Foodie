using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_customer
    {
        [Key]
        public int customer_id { get; set; }

        [Required(ErrorMessage = "Email Is Required")]
        public string email { get; set; }

        [Required(ErrorMessage = "Phone Number Is Required")]
        public string phone { get;set; }

        [Required(ErrorMessage = "Gender Is Required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "ProfilePic Is Required")]
        public List<profilepic> Profilepics { get; set; } = new List<profilepic>();

        [Required(ErrorMessage = "Date Of Birth Is Required")]
        public DateTime DOB { get; set; }
    }
}
