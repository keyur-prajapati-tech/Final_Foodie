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
            if (string.IsNullOrEmpty(email))
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
                string query = @"SELECT vr.restaurant_name,
	                               DATEDIFF(DAY, va.close_DATETIME, va.open_DATETIME) AS OPEN_DAYS,
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


        public void AddUser(tbl_customer tbl_Customer, byte[] profilepic)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"insert into customers.tbl_customer (email,phone,Gender,profilepic,DOB,created_at,updated_at,customer_name,password) values (@email,@phone,@gender,@profilepic,@DOB,@created_at,@updated_at,@customer_name,@passwords)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", tbl_Customer.email);
                cmd.Parameters.AddWithValue("@phone", tbl_Customer.phone);
                cmd.Parameters.AddWithValue("@gender", tbl_Customer.Gender);
                cmd.Parameters.AddWithValue("@profilepic", profilepic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DOB", tbl_Customer.DOB);
                cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_at", DateTime.Now);
                cmd.Parameters.AddWithValue("@customer_name", tbl_Customer.customer_name);
                cmd.Parameters.AddWithValue("@passwords", tbl_Customer.password);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool MenuExists(int menuId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_menu WHERE menu_id = @menu_Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@menu_Id", menuId);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public void AddToCart(tbl_cart_item cart_item)
        {
            if(!MenuExists(cart_item.menu_id))
            {
                throw new Exception("Menu item does not exist.");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                {
                    string query = @"INSERT INTO customers.tbl_cart_item (customer_id, cart_id, quantity, price, menu_id, coupone_id) 
                                 VALUES (@customer_id, @cart_id, @quantity, @price, @menu_id, @coupone_id)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@customer_id", cart_item.customer_id);
                    cmd.Parameters.AddWithValue("@cart_id", cart_item.cart_id);
                    cmd.Parameters.AddWithValue("@quantity", cart_item.quantity);
                    cmd.Parameters.AddWithValue("@price", cart_item.price);
                    cmd.Parameters.AddWithValue("@menu_id", cart_item.menu_id);
                    cmd.Parameters.AddWithValue("@coupone_id", cart_item.coupone_id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public tbl_cart GetOrCreatecart(int customer_id)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM customers.tbl_cart WHERE customer_id = @customer_id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customer_id", customer_id);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    return new tbl_cart
                    {
                        cart_id = Convert.ToInt32(rd["cart_id"]),
                        customer_id = Convert.ToInt32(rd["customer_id"]),
                        TimeStamp = Convert.ToDateTime(rd["TimeStamp"])
                    };
                }
                rd.Close();

                string insertQuery = @"INSERT INTO customers.tbl_cart (customer_id, TimeStamp) VALUES (@customer_id, @TimeStamp)";

                SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@customer_id", customer_id);
                insertCmd.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                int newCartId = Convert.ToInt32(insertCmd.ExecuteScalar());

                return new tbl_cart
                {
                    cart_id = newCartId,
                    customer_id = customer_id,
                    TimeStamp = DateTime.Now
                };
            }
        }

        public List<tbl_cart_item> GetCartItems(int customer_id)
        {
            var items = new List<tbl_cart_item>();

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                string query = @"SELECT * FROM customers.tbl_cart_item WHERE customer_id = @customer_id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customer_id", customer_id);
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    items.Add(new tbl_cart_item
                    {
                        cart_id = Convert.ToInt32(rd["cart_id"]),
                        customer_id = Convert.ToInt32(rd["customer_id"]),
                        quantity = Convert.ToInt32(rd["quantity"]),
                        price = Convert.ToDecimal(rd["price"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        coupone_id = Convert.ToInt32(rd["coupone_id"])
                    });
                }
            }
            return items;
        }

        public void Placeorder(int customer_id)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                var transaction = conn.BeginTransaction();

                try
                {
                    var cartquery = @"SELECT * FROM tbl_cart WHERE customer_id = @cid";
                    SqlCommand cmd = new SqlCommand(cartquery, conn, transaction);
                    cmd.Parameters.AddWithValue("@cid", customer_id);
                    SqlDataReader rd = cmd.ExecuteReader();

                    int cartId = 0;

                    if(rd.Read())
                    {
                        cartId = Convert.ToInt32(rd["cart_id"]);
                    }
                    rd.Close();

                    var AddOrderQuery = @"INSERT INTO tbl_orders (customer_id, order_date, order_status, created_at)  OUTPUT INSERTED.order_id VALUES (@cid, @odate, 'Pending', @created)";
                    SqlCommand cmd1 = new SqlCommand(AddOrderQuery, conn, transaction);
                    cmd1.Parameters.AddWithValue("@cid", customer_id);
                    cmd1.Parameters.AddWithValue("@odate", DateTime.Now);
                    cmd1.Parameters.AddWithValue("@created", DateTime.Now);

                    int orderId = Convert.ToInt32(cmd1.ExecuteScalar());

                    var itemscartitem = @"SELECT * FROM tbl_cart_item WHERE customer_id = @cid";
                    SqlCommand cmd2 = new SqlCommand(itemscartitem, conn, transaction);

                    cmd2.Parameters.AddWithValue("@cid", customer_id);
                    SqlDataReader rd1 = cmd2.ExecuteReader();

                    var items1 = new List<tbl_cart_item>();
                    while (rd1.Read())
                    {
                        items1.Add(new tbl_cart_item
                        {
                            cart_item_id = Convert.ToInt32(rd1["cart_item_id"]),
                            customer_id = Convert.ToInt32(rd1["customer_id"]),
                            cart_id = Convert.ToInt32(rd1["cart_id"]),
                            quantity = Convert.ToInt32(rd1["quantity"]),
                            price = Convert.ToDecimal(rd1["price"]),
                            menu_id = Convert.ToInt32(rd1["menu_id"]),
                            coupone_id = Convert.ToInt32(rd1["coupone_id"])
                        });
                    }
                    rd1.Close();

                    foreach(var items in items1)
                    {
                        var orderitems = @"INSERT INTO tbl_order_items (order_id, menu_id, quantity, price) VALUES (@oid, @menuid, @quantity, @price)";
                        SqlCommand cmd3 = new SqlCommand(orderitems, conn, transaction);
                        cmd3.Parameters.AddWithValue("@oid", orderId);
                        cmd3.Parameters.AddWithValue("@menuid", items.menu_id);
                        cmd3.Parameters.AddWithValue("@quantity", items.quantity);
                        cmd3.Parameters.AddWithValue("@price", items.price);
                        cmd3.ExecuteNonQuery();
                    }

                    var deletecart = @"DELETE FROM tbl_cart_item WHERE customer_id = @cid";
                    SqlCommand cmd4 = new SqlCommand(deletecart, conn, transaction);
                    cmd4.Parameters.AddWithValue("@cid", customer_id);
                    cmd4.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public tbl_menu_items GetMenuItem(int menuId)
        {
            tbl_menu_items menu_Items = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_menu_items WHERE menu_id = @menuId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@menuId", menuId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    menu_Items = new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(reader["menu_id"]),
                        menu_name = reader["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(reader["cuisine_id"]),
                        menu_img = (byte[])reader["menu_img"],
                        menu_descripation = reader["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(reader["amount"]),
                        isAvailable = Convert.ToBoolean(reader["isAvailable"]),
                        Restaurant_id = Convert.ToInt32(reader["Restaurant_id"])
                    };
                }
            }
            return menu_Items;
        }
    }
}
