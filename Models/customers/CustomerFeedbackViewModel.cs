namespace Foodie.Models.customers
{
    public class CustomerFeedbackViewModel
    {
        public List<tbl_customer_feedback> RecentFeedbacks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
    }
}
