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
                        restaurant_street = dr["restaurant_street"].ToString(),
                        restaurant_pincode = dr["restaurant_pincode"].ToString()


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

        public int AddVendors_img(byte[] r_img, byte[] m_img)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"insert into vendores.tbl_vendores_img(Restaurant_id,Restaurant_img,Restaurant_menu_img) values (@Restaurant_id,@Restaurant_img,@Restaurant_menu_img)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Restaurant_id", getRId());
                cmd.Parameters.AddWithValue("@Restaurant_img",r_img);
                cmd.Parameters.AddWithValue("@Restaurant_menu_img", m_img);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public int AddVendor_Avalaiable(tbl_vendor_availability tbl_Vendor_Availability)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                
                string qry = "insert into vendores.tbl_vendor_availability(Restaurant_id,open_time,close_time,day_of_week,is_Open) values(@resid,@o_time,@c_time,@dyw,@open)";

                SqlCommand cmd = new SqlCommand(qry, conn);


                cmd.Parameters.AddWithValue("@resid", getRId());
                cmd.Parameters.AddWithValue("@o_time", tbl_Vendor_Availability.open_time);
                cmd.Parameters.AddWithValue("@c_time", tbl_Vendor_Availability.close_time);
                cmd.Parameters.AddWithValue("@dyw", tbl_Vendor_Availability.day_of_week);
                cmd.Parameters.AddWithValue("@open", tbl_Vendor_Availability.is_Open);
                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;

            }
           
        }

        public int AddPanDetails(tbl_pan_details pan, byte[] img)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring)) 
            {
                string qry = "insert into vendores.tbl_pan_details(pan_number,pan_holder_name,is_Verify,Restaurant_id,img) values(@number,@name,@verify,@rid,@img)";

                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@number", pan.pan_number);
                cmd.Parameters.AddWithValue("@name", pan.pan_holder_name);
                cmd.Parameters.AddWithValue("@verify", pan.is_Verify);
                cmd.Parameters.AddWithValue("@rid",pan.Restaurant_id);
                cmd.Parameters.AddWithValue("@img", img);

                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;
            }
        }

        public int AddGSTDetails(tbl_gst_details gst)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "insert into vendores.tbl_gst_details(gst_number,is_Verify,Restaurant_id) values(@number,@verify,@rid)";

                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@number", gst.gst_number);
                cmd.Parameters.AddWithValue("@verify", gst.is_Verify);
                cmd.Parameters.AddWithValue("@rid", gst.Restaurant_id);

                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;
            }
        }

        public int AddFssaiDetails(tbl_fssai_Details fssai, byte[] img)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "insert into vendores.tbl_fssai_Details(fssai_certi,is_Verify,Restaurant_id,Ex_date,Img) values(@certi,@verify,@rid,@ex,@img)";

                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@certi", fssai.fssai_certi);
                cmd.Parameters.AddWithValue("@verify", fssai.is_Verify);
                cmd.Parameters.AddWithValue("@rid", fssai.Restaurant_id);
                cmd.Parameters.AddWithValue("@ex", fssai.Ex_Date);
                cmd.Parameters.AddWithValue("@img", img);

                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;


            }
        }

        public int AddBankDetails(tbl_bank_details bank)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "insert into vendores.tbl_bank_details(bank_name,IFSC_code,ACC_No,Restaurant_id,ACC_Type) values(@name,@IFSC,@ACC,@rid,@accType)";

                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@name", bank.bank_name);
                cmd.Parameters.AddWithValue("@IFSC", bank.IFSC_code);
                cmd.Parameters.AddWithValue("@ACC", bank.ACC_NO);
                cmd.Parameters.AddWithValue("@rid", bank.Restaurant_id);
                cmd.Parameters.AddWithValue("@accType", bank.ACC_Type);

                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;
            }
        }

    }
}
