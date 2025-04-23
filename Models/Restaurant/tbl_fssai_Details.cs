namespace Foodie.Models.Restaurant
{
    public class tbl_fssai_Details
    {
        public int fssai_id { get; set; }
        public string fssai_certi { get; set; }
        public bool is_Verify { get; set; }
        public int Restaurant_id { get; set; }

        public DateTime Ex_Date { get; set; }
        public byte[] img { get; set; }
    }
}
