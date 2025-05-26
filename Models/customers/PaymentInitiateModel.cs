
namespace Foodie.Models.customers
{
    public class PaymentInitiateModel
    {
        internal int res_id;

        public decimal amount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public int address_id { get; set; }
    }
}
