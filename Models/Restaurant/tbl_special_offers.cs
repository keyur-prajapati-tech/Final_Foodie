using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Foodie.Models.Restaurant
{
    public class tbl_special_offers
    {
        [Key]
        public int so_id { get; set; }

        [Required]
        public int restaurant_id { get; set; }

        [Required]
        public string offer_title { get; set; }
        
        public string offer_desc{ get; set; }


        public int percentage_disc { get; set; }

        public DateTime validFrom { get; set; }

        public DateTime validTo { get; set; }


        public bool is_Active{ get; set; }
        public int menu_id { get; set; }


        public string ImagePath { get; set; } // Comma-separated paths

        [NotMapped]
        public List<IFormFile> image_path { get; set; }
    }
}
