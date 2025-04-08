using Foodie.Models.customers;

namespace Foodie.Repositories
{
    public interface IcustomerRepository
    {
        IEnumerable<tbl_city> GetCitiesByDistrictName(string districtName);

        bool updateProfile(tbl_customer tbl_Customer);
    }
}
