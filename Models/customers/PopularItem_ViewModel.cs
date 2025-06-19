namespace Foodie.Models.customers
{
    public class PopularItem_ViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int OrderCount { get; set; }
        public decimal Price { get; set; }
    }
}
