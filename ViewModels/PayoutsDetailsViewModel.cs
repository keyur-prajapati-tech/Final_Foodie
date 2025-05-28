using Foodie.Models.customers;
using Foodie.Models.Restaurant;

namespace Foodie.ViewModels
{
    public class PayoutsDetailsViewModel
    {
        public tbl_bank_details BankDetails { get; set; } = new tbl_bank_details();
        public int WeekNumber { get; set; }
        public decimal TotalAmount { get; set; }

        List<payments> payments { get; set; } = new List<payments>();
        List<tbl_orders> orders { get; set; } = new List<tbl_orders>();
        List<EarningSummaryViewModel> earnings { get; set; } = new List<EarningSummaryViewModel>();
    }
}
