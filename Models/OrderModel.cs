using System.Collections.Generic;

namespace Foodie.Models
{
    public class OrderModel
    {
        public List<OrderItem> Items { get; set; }
        public int Total { get; set; }
    }

    public class OrderItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
