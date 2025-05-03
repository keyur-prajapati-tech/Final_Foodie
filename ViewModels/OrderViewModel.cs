namespace Foodie.ViewModels
{
    public class OrderViewModel
    {

        
            public int OrderId { get; set; }
            public string customer_name { get; set; }
            public string RestaurantName { get; set; }
            public string order_status { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? DeliveryTime { get; set; }

            public string Menu { get; set; }
        //public decimal Amount { get; set; }
            public int coupone_id { get; set; }

            public decimal discount { get; set; }
        //public string PaymentMethod { get; set; }
        //public string PreparationTimeFormatted { get; set; }
        // public int PreparationTimeMinutes { get; set; }

    }
}
