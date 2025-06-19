namespace Foodie.Models.customers
{
    public class MenuCategoryStatsViewModel
    {
        public string CuisineName { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}
