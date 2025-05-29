using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.DeliveryPartner
{
    public class tbl_signupFees
    {
        [Key]
        public int FeeID { get; set; }
        public int partner_id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionID { get; set; }
        public DateTime PaidAt { get; set; }
        public string Status { get; set; }
    }
}
