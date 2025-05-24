namespace Foodie.ViewModels
{
    public class order
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string OrderStatus { get; set; }
        public List<OrderViewModel> Reports { get; set; } = new List<OrderViewModel>();
    }
}
