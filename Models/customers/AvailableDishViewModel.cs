namespace Foodie.Models.customers
{
    public class AvailableDishViewModel
    {
        public int MenuId { get; set; }
        public string Menudescription { get; set; }
        public string RestaurantName { get; set; }
        public string CuisineName { get; set; }
        public string MenuName { get; set; }
        public decimal Amount { get; set; }
        public bool IsAvailable { get; set; }
        public byte[] RestaurantImage { get; set; }
        public bool IsApprove { get; set; }
    }
}
