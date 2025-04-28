namespace Foodie.ViewModels
{
    public class DashBoardViewModel
    {
            public decimal MonthlySales { get; set; }
            public int MonthlyCustomers { get; set; }
            public int MonthlyRestaurants { get; set; }

            public int CancelledOrders { get; set; }
            public int PendingOrders { get; set; }
            public int AcceptedOrders { get; set; }
            public int DeliverdOrders { get; set; }

            public int ActiveRestaurants { get; set; }
            public int InactiveRestaurants { get; set; }
            public int OpenRestaurants { get; set; }
            public int ClosedRestaurants { get; set; }

        public List<int> Month { get; set; } = new List<int>();
        public List<int> MonthlySalesdata { get; set; } = new List<int>();
        public List<int> Year { get; set; } = new List<int>();
        public List<int> YearlySalesdata { get; set; } = new List<int>();
       

    }
}
