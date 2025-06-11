namespace Foodie.Models.customers
{
    public class CouponeViewModel
    {
        public string coupone_code { get; set; }
        public string description { get; set; }
        public decimal discount { get; set; }
        public decimal MinimumOrderAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsApplicable { get; set; }
        public string ApplicabilityMessage { get; set; }
    }
}
