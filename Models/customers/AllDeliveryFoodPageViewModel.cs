using Foodie.Models.Restaurant;

namespace Foodie.Models.customers
{
    public class AllDeliveryFoodPageViewModel
    {
        public tbl_restaurant Restaurant { get; set; }
        public MenuItemViewModel SeletedMenItem { get; set; }
        public List<AvailableDishViewModel> AvailableDishes { get; set; }
    }
}
