using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_cart
    {
        [Key]
        public int cart_id { get; set; }
        public int customer_id { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
