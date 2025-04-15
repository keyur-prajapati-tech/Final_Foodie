using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodie.Models.customers
{
    public class tbl_customer
    {
        public int customer_id { get; set; }

        [JsonPropertyName("email")]
        public string email { get; set; }

        [JsonPropertyName("phone")]
        public string phone { get;set; }
        public string Gender { get; set; }
        public byte[] profilepic { get; set; }
        public DateTime DOB { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        [JsonPropertyName("uid")]
        public string UID { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }
    }
}
