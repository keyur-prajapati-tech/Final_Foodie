using System.Numerics;

namespace Foodie.Models
{
    public class tbl_admin
    {
        public int admin_id { get; set; }
        public string Full_name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int role_id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public string role_type { get; set; }
        public byte [] IMAGE { get; set; }
    }
}
