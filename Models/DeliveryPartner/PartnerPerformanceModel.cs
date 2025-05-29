namespace Foodie.Models.DeliveryPartner
{
    public class PartnerPerformanceModel
    {
        public string FullName { get; set; }
        public int TotalOrdersDelivered { get; set; }
        public int LateDeliveries { get; set; }
        public int CancellationCount { get; set; }
        public decimal CustomerRatingAvg { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
