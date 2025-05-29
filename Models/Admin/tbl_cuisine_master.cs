namespace Foodie.Models.Admin
{
    public class tbl_cuisine_master
    {
        public int cuisine_id { get; set; }
        public string cuisine_name { get; set; }
        public int item_count { get; set; } // Number of items in this cuisine
    }
}
