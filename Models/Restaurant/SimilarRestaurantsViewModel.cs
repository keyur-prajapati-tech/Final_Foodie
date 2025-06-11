namespace Foodie.Models.Restaurant
{
    public class SimilarRestaurantsViewModel
    {
        public string CurrentRestaurantName { get; set; }
        public List<SimilarRestaurantDtoViewModel> SimilarRestaurants { get; set; }
    }
}
