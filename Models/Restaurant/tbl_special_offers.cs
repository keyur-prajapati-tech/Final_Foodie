using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Foodie.Models.Restaurant
{
    public class tbl_special_offers
    {
        public int SoId { get; set; }
        public int RestaurantId { get; set; }

        [Required]
        [Display(Name = "Offer Title")]
        public string OfferTitle { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string OfferDesc { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Discount %")]
        public int PercentageDisc { get; set; }

        [Required]
        [Display(Name = "Valid From")]
        public DateTime ValidFrom { get; set; }

        [Required]
        [Display(Name = "Valid To")]
        public DateTime ValidTo { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public int MenuId { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        public IFormFile OfferImage { get; set; }
    }
}
