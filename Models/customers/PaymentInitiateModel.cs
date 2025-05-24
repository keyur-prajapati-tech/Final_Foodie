
namespace Foodie.Models.customers
{
    public class PaymentInitiateModel
    {
        public decimal amount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public int address_id { get; set; }
    }
}
