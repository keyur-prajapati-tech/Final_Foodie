namespace Foodie.Models.Restaurant
{
    public class tbl_vendor_availability
    {
        public int ven_ava_id { get; set; }
        public int Restaurant_id { get; set; }
        public DateTime open_time { get; set; }
        public DateTime close_time { get; set; }

        public string day_of_week { get; set; }

        public bool is_Open { get; set; }

    }
}
