namespace Foodie.Models.Restaurant
{
    public class tbl_pan_details
    {
        public int pan_id { get; set; }
        public string pan_number { get; set; }
        public string pan_holder_name { get; set; }

        public bool is_Verify { get; set; }

        public int Restaurant_id { get; set; }
        public byte[] img { get; set; }
    }
}
