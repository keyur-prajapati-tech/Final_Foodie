using Foodie.Models.Restaurant;
using Microsoft.Data.SqlClient;


namespace Foodie.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly string _connectionstring;
        private int o_id = 0;
        private static  int r_id = 0;
        public static void setRId(int id)
        {
            r_id = id;
        }

        public static int getRId()
        {
            return r_id;
        }
        public RestaurantRepository(IConfiguration configuration)
        {
            _connectionstring = configuration.GetConnectionString("Defaultconnection");
        }

        public int AddOwner(tbl_owner_details owner)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO vendores.tbl_owner_details (owner_name, owner_email, owner_contact) 
                                 VALUES (@owner_name, @owner_email, @owner_contact);
                                   SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@owner_name", owner.owner_name);
                cmd.Parameters.AddWithValue("@owner_email", owner.owner_email);
                cmd.Parameters.AddWithValue("@owner_contact", owner.owner_contact);
                conn.Open();
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                o_id = result;
                conn.Close();
                return result;
            }
        }

        public int AddRestaurant(tbl_restaurant restaurant)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO vendores.tbl_restaurant (restaurant_name, restaurant_contact, restaurant_email, restaurant_street, restaurant_pincode, restaurant_lat, restaurant_lag, restaurant_isActive,est_id,owner_id) 
                                 VALUES (@restaurant_name, @restaurant_contact, @restaurant_email, @restaurant_street, @restaurant_pincode, @restaurant_lat, @restaurant_lag, @restaurant_isActive,1,@owner_id);
                                    SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@restaurant_name", restaurant.restaurant_name);
                cmd.Parameters.AddWithValue("@restaurant_contact", restaurant.restaurant_contact);
                cmd.Parameters.AddWithValue("@restaurant_email", restaurant.restaurant_email);
                cmd.Parameters.AddWithValue("@restaurant_street", restaurant.restaurant_street);
                cmd.Parameters.AddWithValue("@restaurant_pincode", restaurant.restaurant_pincode);
                cmd.Parameters.AddWithValue("@restaurant_lat", restaurant.restaurant_lat);
                cmd.Parameters.AddWithValue("@restaurant_lag", restaurant.restaurant_lag);
                cmd.Parameters.AddWithValue("@restaurant_isActive", restaurant.restaurant_isActive);
                cmd.Parameters.AddWithValue("@owner_id", o_id);

                conn.Open();
                int result = Convert.ToInt32(cmd.ExecuteScalar());

                setRId(result);

                conn.Close();
                return result;
            }
        }

        public tbl_restaurant getRestaurant(int id)
        {
            tbl_restaurant tbl_Restaurant = null;
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_restaurant where restaurant_id = @id";

                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();

                SqlDataReader dr =  cmd.ExecuteReader();

                if(dr.Read())
                {
                    tbl_Restaurant = new tbl_restaurant
                    {
                        restaurant_name = dr["restaurant_name"].ToString(),
                        restaurant_contact = dr["restaurant_contact"].ToString(),
                        restaurant_email = dr["restaurant_email"].ToString(),


                    };
                }
                conn.Close();
                return tbl_Restaurant;
            }
        }

        public tbl_owner_details getOwner(int id)
        {
            tbl_owner_details tbl_Owner_Details = null;
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select o.* from vendores.tbl_restaurant  r inner join vendores.tbl_owner_details o on o.owner_id= r.owner_id where restaurant_id = @id";

                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    tbl_Owner_Details = new tbl_owner_details
                    {
                        owner_name = dr["owner_name"].ToString(),
                        owner_contact = dr["owner_contact"].ToString(),
                        owner_email = dr["owner_email"].ToString(),


                    };
                }
                conn.Close();
                return tbl_Owner_Details;
            }
        }
    }
}
