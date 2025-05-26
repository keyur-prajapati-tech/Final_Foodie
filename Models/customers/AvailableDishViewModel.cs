namespace Foodie.Models.customers
{
    public class AvailableDishViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string Menudescription { get; set; }
        public decimal amount { get; set; }
        public string RestaurantName { get; set; }
        public string CuisineName { get; set; }
        public decimal Amount { get; set; }
        public bool IsAvailable { get; set; }
        public string RestaurantImagebase64 { get; set; }
        public bool IsApprove { get; set; }
        public int discountPercentage { get; set; }
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        public string RestaurantStreet { get; set; }
        public string RestaurantPincode { get; set; }
        public string RestaurantContact { get; set; }
        public byte[] RestaurantImageBytes { get; set; }
    }
}
