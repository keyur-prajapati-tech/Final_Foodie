namespace Foodie.Models.customers
{
    public class CuisineStats
    {
        public string CuisineName { get; set; } // Changed from CategoryName
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}
