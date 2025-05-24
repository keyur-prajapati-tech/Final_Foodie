namespace Foodie.Models.customers
{
    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public byte[] MenuImg { get; set; }
        public string MenuDescription { get; set; }
        public decimal Amount { get; set; }
        public string cuisine_name { get; set; }

        public int cuisine_id { get; set; }

        public int RestaurantId { get; set; }
        public bool IsAvalable { get; set; }

        public string MenuImageBase64 { get; set; }
    }
}
