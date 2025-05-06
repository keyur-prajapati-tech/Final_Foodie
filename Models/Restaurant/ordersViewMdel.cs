namespace Foodie.Models.Restaurant
{
    public class ordersViewMdel
    {
        public int order_items_id { get; set; }
        public int order_id { get; set; }
        public int restaurant_id { get; set; }

       
        public string food_status { get; set; }

        public string customer_name { get; set; }
        public List<orderNotiViewModel> Dishes { get; set; }
    }
}
