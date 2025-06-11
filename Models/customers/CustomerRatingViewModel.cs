namespace Foodie.Models.customers
{
    public class CustomerRatingViewModel
    {
        public int RatingId { get; set; }
        public int CustomerId { get; set; }
        public int RestaurantId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
    }
}
