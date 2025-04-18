namespace Foodie.Models.customers
{
    public class RegisterViewModel
    {
        public tbl_customer Customer { get; set; }
        public List<tbl_city> Cities { get; set; }
        public string DistrictName { get; set; }
    }
}
