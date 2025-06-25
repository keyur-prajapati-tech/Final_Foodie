namespace Foodie.Models.DeliveryPartner
{
    public class tbl_problem_reports
    {
        public int problem_id {  get; set; }
        public int order_id { get; set; }
        public int partner_id {  get; set; }
        public string description {  get; set; }
        public string status { get; set; }
    }
}
