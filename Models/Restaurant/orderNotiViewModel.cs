namespace Foodie.Models.Restaurant
{
    public class orderNotiViewModel
    {
        public string  menu_name { get; set; }
        public int quantity { get; set; }
        public decimal list_price { get; set; }
        public decimal discount { get; set; }

        public DateTime estimated_time { get; set; }
    }
}
