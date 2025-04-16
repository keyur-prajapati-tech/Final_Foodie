using Foodie.Models.customers;
using Foodie.Models.Restaurant;

namespace Foodie.Repositories
{
    public interface IcustomerRepository
    {
        IEnumerable<tbl_city> GetCitiesByDistrictName(string districtName);

        tbl_customer GetTbl_Customer(string email);

        void AddUser(tbl_customer tbl_Customer);

        bool updateProfile(tbl_customer tbl_Customer, byte[] profilepic);

        List<RestaurantInfo> GetAllRestaurants();
    }
}
