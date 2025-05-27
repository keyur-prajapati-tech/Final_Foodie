using Foodie.Models.Restaurant;

namespace Foodie.ViewModels
{
    public class PayoutsDetailsViewModel
    {
        public tbl_bank_details BankDetails { get; set; } = new tbl_bank_details();
        public int WeekNumber { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
