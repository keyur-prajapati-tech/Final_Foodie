using Foodie.Models.customers;

namespace Foodie.Models.Restaurant
{
    public class RestaurantInfo
    {
        public int menu_id { get; set; }
        public string Restaurant_name { get; set; }
        public int DayOfWeek { get; set; }
        public byte[] Restaurant_img { get; set; }

        public int RestaurantId { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }

        public string restaurant_street { get; set; }
        public string pincode { get; set; }


        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal Rating { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }

        public bool isApproved { get; set; }
    }
}
