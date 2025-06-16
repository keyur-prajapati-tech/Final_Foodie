namespace Foodie.Models.customers
{
    public class PopularItem_ViewModel
    {
        public string ItemName { get; set; }
        public string CuisineName { get; set; }
        public int OrderCount { get; set; }
        public decimal Price { get; set; }
        public string ImageBase64 { get; set; } // Base64 string for the image
    }
}
