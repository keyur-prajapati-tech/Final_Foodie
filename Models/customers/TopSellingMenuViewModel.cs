
namespace Foodie.Models.customers
{
    public class TopSellingMenuViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string ImageUrl { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public decimal list_price { get; set; }
        public string MenuDescription { get; set; }
    }
}
