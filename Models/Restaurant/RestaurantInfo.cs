namespace Foodie.Models.Restaurant
{
    public class RestaurantInfo
    {
        public int menu_id { get; set; }
        public string Restaurant_name { get; set; }
        public int DayOfWeek { get; set; }
        public byte[] Restaurant_img { get; set; }
    }
}
