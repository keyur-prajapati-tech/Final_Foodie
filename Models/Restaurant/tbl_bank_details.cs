namespace Foodie.Models.Restaurant
{
    public class tbl_bank_details
    {
        public int bank_id { get; set; }
        public string bank_name { get; set; }
        public string IFSC_code { get; set; }
        public string ACC_NO { get; set; }
        public int Restaurant_id { get; set; }
        public string ACC_Type { get; set; }
    }
}
