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

        public tbl_customer GetTbl_Customer(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return null;
            }

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
                        customer_name = reader["customer_name"].ToString(),
                        created_at = Convert.ToDateTime(reader["created_at"]),
                        updated_at = Convert.ToDateTime(reader["updated_at"])
                    };
                }
            }

            return customer;
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

        public void AddUser(tbl_customer customer, byte[] profilepic)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = @"
            INSERT INTO customers.tbl_customer 
            (email, phone, Gender, profilepic, dob, created_at, updated_at,customer_name,password) 
            VALUES (@email, @phone, @gender, @profilepic, @dob, @createdAt, @updatedAt, @customername, @password)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", customer.email ?? "");
                cmd.Parameters.AddWithValue("@phone", customer.phone ?? "");
                cmd.Parameters.AddWithValue("@gender", customer.Gender ?? "");
                cmd.Parameters.AddWithValue("@profilepic", customer.profilepic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@dob", customer.DOB);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@customername", customer.customer_name ?? "");
                cmd.Parameters.AddWithValue("@password", customer.password ?? "");

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public tbl_customer ValidateCustomerLogin(string email, string password)
        {
            tbl_customer customer = null;

            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM customers.tbl_customer WHERE email = @Email AND password = @Password";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password); // ⚠️ Consider hashing passwords in production

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    customer = new tbl_customer
                    {
                        customer_id = Convert.ToInt32(reader["customer_id"]),
                        email = reader["email"].ToString(),
                        customer_name = reader["customer_name"].ToString(),
                        phone = reader["phone"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DOB = Convert.ToDateTime(reader["dob"]),
                        created_at = Convert.ToDateTime(reader["created_at"]),
                        updated_at = Convert.ToDateTime(reader["updated_at"]),
                        password = reader["password"].ToString()
                    };
                }
            }

            return customer;
        }

        public void UpdateCustomerProfile(tbl_customer customerinfo, byte[] profilePic)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = @"
                    UPDATE customers.tbl_customer 
                    SET customer_name = @customer_name,
                        phone = @phone,
                        Gender = @gender,
                        DOB = @dob,
                        updated_at = @updatedAt,
                        profilepic = @profilepic
                    WHERE email = @Email";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@customer_name", customerinfo.customer_name ?? "");
                cmd.Parameters.AddWithValue("@phone", customerinfo.phone ?? "");
                cmd.Parameters.AddWithValue("@gender", customerinfo.Gender ?? "");
                cmd.Parameters.AddWithValue("@dob", customerinfo.DOB);
                cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@profilepic", customerinfo.profilepic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", customerinfo.email);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
