namespace Foodie.Models.customers
{
    public class MenuStatsResponse
    {
        public List<MenuCategoryStatsViewModel> CategoryStats { get; set; }
        public MenuCategoryStatsViewModel Totals { get; set; }
        public string TimePeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
