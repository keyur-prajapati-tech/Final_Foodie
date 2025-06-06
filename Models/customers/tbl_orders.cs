﻿using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_orders
    {
        public int order_id { get; set; }
        public int customer_id { get; set; }
        public DateTime order_date { get; set; } = DateTime.Now;
        public string order_status { get; set; } = "Pending";
        public decimal grand_total { get; set; }
        public string razorpay_payment_id { get; set; }
        public string razorpay_order_id { get; set; }
        public string razorpay_signature { get; set; }
        public int address_id { get; set; }

        public List<tbl_order_items> OrderItems { get; set; } = new List<tbl_order_items>();
    }
}
