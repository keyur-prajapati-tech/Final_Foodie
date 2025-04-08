﻿using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.customers
{
    public class tbl_customer
    {
        public int customer_id { get; set; }
        public string email { get; set; }
        public string phone { get;set; }
        public string Gender { get; set; }
        public string profilepic { get; set; }
        public DateTime DOB { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string UID { get; set; }
        public string name { get; set; }
    }
}
