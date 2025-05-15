namespace Foodie.Models.customers
{
    public class tbl_ratings
    {
       
            public int RatingId { get; set; }
            public int? CustomerId { get; set; }
            public string customer_name { get; set; }
            public int? RestaurantId { get; set; }
            public string restaurant_name { get; set; }
            public int? OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public int RatingValue { get; set; }
            public string discription { get; set; }
            public byte[] image { get; set; }


    }
}
