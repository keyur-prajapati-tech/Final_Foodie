namespace Foodie.Models.customers
{
    public class tbl_coupone
    {
            public int coupone_id { get; set; }
            public string coupone_code { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public decimal discount { get; set; }
            public decimal application_order_amount { get; set; }
            public DateTime expiry_date { get; set; }
    }
}
