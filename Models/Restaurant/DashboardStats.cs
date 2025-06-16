namespace Foodie.Models.Restaurant
{
    public class DashboardStats
    {
        public int ActiveOrders { get; set; }
        public decimal TodaysRevenue { get; set; }
        public int NewCustomers { get; set; }
        public int MenuItems { get; set; }
        public int LowStockItems { get; set; }



        //// Hardcoded as shown in your expected output
        //public int FeaturedItems { get; set; } = 12;

        //// From tbl_orders (today's count)
        //public int NewOrdersToday { get; set; }

        //// Calculated (today vs yesterday)
        //public decimal RevenueChangePercentage { get; set; }

        //// From tbl_customer (last 7 days)
        //public int NewCustomersThisWeek { get; set; }

        //// Hardcoded 15% as shown in your expected output
        //public double ConversionRate { get; set; } = 15.0;
    }
}
