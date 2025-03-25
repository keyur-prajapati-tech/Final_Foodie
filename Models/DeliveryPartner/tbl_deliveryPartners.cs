using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Foodie.Models
{
    [Table("tbl_deliveryPartners", Schema = "deliverypartner")]
    public class tbl_deliveryPartners
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Partner_Id { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact Number must be exactly 10 digits.")]
        public long ContactNumber { get; set; }


        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20)]
        public string Status { get; set; } // Pending, Approved, Rejected

        [Required]
        public DateTime CreatedAT { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool IsAvailable { get; set; } = false;


        
    }
}
