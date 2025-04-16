using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Microsoft.Data.SqlClient;

namespace Foodie.Repositories
{
    public class customerRepository : IcustomerRepository
    {
        private readonly string _connectionstring;

        public customerRepository(IConfiguration configuration)
        {
            _connectionstring = configuration.GetConnectionString("Defaultconnection");
        }

        public IEnumerable<tbl_city> GetCitiesByDistrictName(string districtName)
        {
            var tbl_cities = new List<tbl_city>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT cc.* 
                FROM customers.tbl_city cc
                JOIN customers.tbl_district cd ON cc.DistrictId = cd.DistrictId
                WHERE cd.DisrictName = 'surat';";

                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    tbl_cities.Add(new tbl_city
                    {
                        CityId = (int)rd["CityId"],
                        CityName = rd["CityName"].ToString(),
                        DistrictId = (int)rd["DistrictId"]
                    });
                }
                conn.Close();
            }
            return tbl_cities;
        }

        public void AddUser(tbl_customer customer)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = @"
            INSERT INTO customers.tbl_customer 
            (email, name, UID, created_at, updated_at) 
            VALUES (@Email, @Name, @UID, @CreatedAt, @UpdatedAt)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", customer.email ?? "");
                cmd.Parameters.AddWithValue("@Name", customer.name ?? "");
                cmd.Parameters.AddWithValue("@UID", customer.UID ?? "");
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public tbl_customer GetTbl_Customer(string email)
        {
            tbl_customer customer = null;

            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM customers.tbl_customer WHERE email = @Email";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", email);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    customer = new tbl_customer
                    {
                        customer_id = Convert.ToInt32(reader["customer_id"]),
                        email = reader["email"].ToString(),
                        name = reader["name"].ToString(),
                        UID = reader["UID"].ToString(),
                        created_at = Convert.ToDateTime(reader["created_at"]),
                        updated_at = Convert.ToDateTime(reader["updated_at"])
                    };
                }
            }

            return customer;
        }


        public bool updateProfile(tbl_customer tbl_Customer, byte[] profilepic)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {

                DateTime created_at = DateTime.Now;
                DateTime updated_at = DateTime.Now;

                string query = @"INSERT INTO customers.tbl_customer (email,phone,Gender,profilepic,DOB,created_at,updated_at,name) VALUES (@email,@phone,@Gender,@profilepic,@DOB,@created_at,@updated_at,@name)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", tbl_Customer.email);
                cmd.Parameters.AddWithValue("@phone", tbl_Customer.phone);
                cmd.Parameters.AddWithValue("@Gender", tbl_Customer.Gender);
                cmd.Parameters.AddWithValue("@profilepic", tbl_Customer.profilepic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DOB", tbl_Customer.DOB);
                cmd.Parameters.AddWithValue("@created_at", created_at);
                cmd.Parameters.AddWithValue("@updated_at", updated_at);
                cmd.Parameters.AddWithValue("@name", tbl_Customer.name);

                conn.Open();
                int rowsEffected = cmd.ExecuteNonQuery();
                conn.Close();

                return rowsEffected > 0;
            }
        }

        public List<RestaurantInfo> GetAllRestaurants()
        {
            List<RestaurantInfo> restaurantInfos = new List<RestaurantInfo>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
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
