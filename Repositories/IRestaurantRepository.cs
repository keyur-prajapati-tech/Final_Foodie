using Foodie.Models.Restaurant;

namespace Foodie.Repositories
{
    public interface IRestaurantRepository
    {
        public int AddOwner(tbl_owner_details owner);
        public int AddRestaurant(tbl_restaurant restaurant);

        public tbl_restaurant getRestaurant(int id);

        public tbl_owner_details getOwner(int id);
    }
}
