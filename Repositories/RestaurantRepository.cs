using Foodie.Models.Restaurant;
using Microsoft.Data.SqlClient;

namespace Foodie.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly string _connectionstring;

        public RestaurantRepository(IConfiguration configuration)
        {
            _connectionstring = configuration.GetConnectionString("Defaultconnection");
        }

        public int AddOwner(tbl_owner_details owner)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO vendores.tbl_owner_details (owner_name, owner_email, owner_contact) 
                                 VALUES (@owner_name, @owner_email, @owner_contact);";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@owner_name", owner.owner_name);
                cmd.Parameters.AddWithValue("@owner_email", owner.owner_email);
                cmd.Parameters.AddWithValue("@owner_contact", owner.owner_contact);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public int AddRestaurant(tbl_restaurant restaurant)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO vendores.tbl_restaurant (restaurant_name, restaurant_contact, restaurant_email, restaurant_street, restaurant_pincode, restaurant_lat, restaurant_lag, restaurant_isActive) 
                                 VALUES (@restaurant_name, @restaurant_contact, @restaurant_email, @restaurant_street, @restaurant_pincode, @restaurant_lat, @restaurant_lag, @restaurant_isActive);";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@restaurant_name", restaurant.restaurant_name);
                cmd.Parameters.AddWithValue("@restaurant_contact", restaurant.restaurant_contact);
                cmd.Parameters.AddWithValue("@restaurant_email", restaurant.restaurant_email);
                cmd.Parameters.AddWithValue("@restaurant_street", restaurant.restaurant_street);
                cmd.Parameters.AddWithValue("@restaurant_pincode", restaurant.restaurant_pincode);
                cmd.Parameters.AddWithValue("@restaurant_lat", restaurant.restaurant_lat);
                cmd.Parameters.AddWithValue("@restaurant_lag", restaurant.restaurant_lag);
                cmd.Parameters.AddWithValue("@restaurant_isActive", restaurant.restaurant_isActive);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public List<RestaurantInfo> GetAllRestaurants()
        {
            List<RestaurantInfo> restaurantInfos = new List<RestaurantInfo>();

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT 
                            vr.restaurant_name,
                            DATEDIFF(DAY, va.close_time, va.open_time) AS OPEN_DAYS,
                            vi.Restaurant_img
                         FROM 
                            vendores.tbl_restaurant vr
                         JOIN 
                            vendores.tbl_vendor_availability va ON vr.restaurant_id = va.Restaurant_id
                         JOIN 
                            vendores.tbl_vendores_img vi ON vr.restaurant_id = vi.Restaurant_id
                         WHERE va.is_Open = 1";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    RestaurantInfo list = new RestaurantInfo
                    {
                        Restaurant_name = reader["restaurant_name"].ToString(),
                        DayOfWeek = (int)reader["OPEN_DAYS"],
                        Restaurant_img = (byte[])reader["Restaurant_img"]
                    };
                    restaurantInfos.Add(list);
                }
            }
            return restaurantInfos;
        }
    }
}
