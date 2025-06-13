namespace Foodie.Models.customers
{
    public class DeliveryPartnerInfo
    {
        public int delivery_partner_id { get; set; }
        public string delivery_partner_name { get; set; }
        public string delivery_partner_phone { get; set; }
        public string delivery_partner_email { get; set; }
        public string delivery_partner_image { get; set; }
        public string vehicle_type { get; set; }
        public string vehicle_number { get; set; }
        public DateTime? assigned_date { get; set; }
        public DateTime? delivered_date { get; set; }
        public int order_id { get; set; }
        public int order_status { get; set; }

        public string PhotoUrl { get; set; }
    }
}
