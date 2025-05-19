using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Foodie.Models.customers.tbl_coupone;
using System.Data;
using System;
using Foodie.Models.Admin;

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
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT customer_id, customer_name, email FROM customers.tbl_customer WHERE email = @Email AND password = @Password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new tbl_customer
                        {
                            customer_id = reader.GetInt32(0),
                            customer_name = reader.GetString(1),
                            email = reader.GetString(2)
                        };
                    }
                }
            }
            return null;
        }

        public tbl_restaurant ValidateRestaurantLogin(string email, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT restaurant_id, restaurant_name, restaurant_email FROM vendores.tbl_restaurant WHERE restaurant_email = @Email AND password = @Password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new tbl_restaurant
                        {
                            restaurant_id = reader.GetInt32(0),
                            restaurant_name = reader.GetString(1),
                            restaurant_email = reader.GetString(2)
                        };
                    }
                }
            }
            return null;
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

        public List<tbl_menu_items> GetAllMenuItems()
        {
            List<tbl_menu_items> menuItems = new List<tbl_menu_items>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_menu_items";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    menuItems.Add(new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        menu_name = rd["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(rd["cuisine_id"]),
                        menu_img = (byte[])rd["menu_img"],
                        menu_descripation = rd["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(
                            rd["amount"]),
                        isAvailable = Convert.ToBoolean(rd["isAvalable"]) ? true : false,
                        Restaurant_id = Convert.ToInt32(rd["Restaurant_id"])
                    });
                }
                conn.Close();
            }
            return menuItems;
        }

        [HttpPost]
        public void AddToCart(tbl_cart_item tbl_Cart_Item)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO customers.tbl_cart_item (cart_id, quantity, price, menu_id, coupone_id) 
                                 VALUES (@cart_id, @quantity, @price, @menu_id, @coupone_id)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cart_id", tbl_Cart_Item.cart_id);
                cmd.Parameters.AddWithValue("@quantity", tbl_Cart_Item.quantity);
                cmd.Parameters.AddWithValue("@price", tbl_Cart_Item.price);
                cmd.Parameters.AddWithValue("@menu_id", tbl_Cart_Item.menu_id);
                cmd.Parameters.AddWithValue("@coupone_id", tbl_Cart_Item.coupone_id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        [HttpGet]
        public tbl_cart GetOrCreateCart(int customerId)
        {
            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM customers.tbl_cart WHERE customer_id = @customerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    return new tbl_cart
                    {
                        cart_id = Convert.ToInt32(rd["cart_id"]),
                        customer_id = Convert.ToInt32(rd["customer_id"]),
                        DATETIME = Convert.ToDateTime(rd["DATETIME"])
                    };
                }
                rd.Close();

                //Create a new cart if it doesn't exist
                var insertCartQuery = @"INSERT INTO customers.tbl_cart (customer_id, DATETIME) 
                        VALUES (@customerId, @datetime);
                        SELECT SCOPE_IDENTITY();";

                SqlCommand insertCmd = new SqlCommand(insertCartQuery, conn);
                insertCmd.Parameters.AddWithValue("@customerId", customerId);
                insertCmd.Parameters.AddWithValue("@datetime", DateTime.Now);

                object result = insertCmd.ExecuteScalar();
                int newCartId = Convert.ToInt32(result);

                return new tbl_cart
                {
                    cart_id = newCartId,
                    customer_id = customerId,
                    DATETIME = DateTime.Now
                };
            }
        }

        public List<CartItemViewModel> GetCartItems(int customerId)
        {
            var items = new List<CartItemViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"
        SELECT ci.cart_item_id, ci.menu_id, ci.quantity, ci.price,
               mi.menu_name, mi.menu_img
        FROM customers.tbl_cart_item ci
        INNER JOIN customers.tbl_cart c ON ci.cart_id = c.cart_id
        INNER JOIN vendores.tbl_menu_items mi ON ci.menu_id = mi.menu_id
        WHERE c.customer_id = @customerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                conn.Open();

                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    items.Add(new CartItemViewModel
                    {
                        cart_item_id = Convert.ToInt32(rd["cart_item_id"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        quantity = Convert.ToInt32(rd["quantity"]),
                        amount = Convert.ToDecimal(rd["price"]),
                        menu_name = rd["menu_name"].ToString(),
                        menu_img = (byte[])rd["menu_img"]
                    });
                }

                conn.Close();
            }

            return items;
        }

        //public void PlaceOrder(int customerId)
        //{
        //    using(SqlConnection conn = new SqlConnection(_connectionstring))
        //    {
        //        conn.Open();

        //        var transaction = conn.BeginTransaction();

        //        try
        //        {
        //            var cartquery = "SELECT * FROM customers.tbl_cart WHERE customer_id = @customerId";
        //            SqlCommand cartCmd = new SqlCommand(cartquery, conn, transaction);
        //            cartCmd.Parameters.AddWithValue("@customerId", customerId);

        //            SqlDataReader cartReader = cartCmd.ExecuteReader();

        //            int cartId = 0;
        //            if (cartReader.Read())
        //            {
        //                cartId = Convert.ToInt32(cartReader["cart_id"]);
        //            }
        //            cartReader.Close();

        //            var insertOrderQuery = @"INSERT INTO tbl_orders (customer_id, order_date, order_status, created_at) OUTPUT INSERTED.order_id VALUES (@cid, @odate, 'Pending', @created";
        //            SqlCommand insertOrderCmd = new SqlCommand(insertOrderQuery, conn, transaction);

        //            insertOrderCmd.Parameters.AddWithValue("@cid", customerId);
        //            insertOrderCmd.Parameters.AddWithValue("@odate", DateTime.Now);
        //            insertOrderCmd.Parameters.AddWithValue("@created", DateTime.Now);

        //            int orderId = (int)insertOrderCmd.ExecuteScalar();

        //            var cartItemsQuery = "SELECT * FROM customers.tbl_cart_item WHERE cart_id = @cartId";
        //            SqlCommand cartItemsCmd = new SqlCommand(cartItemsQuery, conn, transaction);
        //            cartItemsCmd.Parameters.AddWithValue("@cartId", cartId);

        //            SqlDataReader cartItemsReader = cartItemsCmd.ExecuteReader();

        //            var items = new List<tbl_cart_item>();
        //            while (cartItemsReader.Read()){
        //                items.Add(new tbl_cart_item
        //                {
        //                    cart_item_id = Convert.ToInt32(cartItemsReader["cart_item_id"]),
        //                    cart_id = Convert.ToInt32(cartItemsReader["cart_id"]),
        //                    quantity = Convert.ToInt32(cartItemsReader["quantity"]),
        //                    price = Convert.ToDecimal(cartItemsReader["price"]),
        //                    menu_id = Convert.ToInt32(cartItemsReader["menu_id"]),
        //                    coupone_id = Convert.ToInt32(cartItemsReader["coupone_id"])
        //                });
        //            }
        //            cartItemsReader.Close();

        //            foreach(var item in items)
        //            {
        //                var insertOrderItemQuery = @"INSERT INTO tbl_order_items 
        //                (order_id, menu_id, quantity, list_price, discount, created_at) 
        //                VALUES (@order_id, @menu_id, @qty, @price, 0.10, @created)";
                        
        //                SqlCommand insertCommand = new SqlCommand(insertOrderItemQuery, conn, transaction);
        //                insertCommand.Parameters.AddWithValue("@order_id", orderId);
        //                insertCommand.Parameters.AddWithValue("@menu_id", item.menu_id);
        //                insertCommand.Parameters.AddWithValue("@qty", item.quantity);
        //                insertCommand.Parameters.AddWithValue("@price", item.price);
        //                insertCommand.Parameters.AddWithValue("@created", DateTime.Now);
        //                insertOrderCmd.ExecuteNonQuery();
        //            }

        //            // Clear the cart after placing the order
        //            var clearCart = new SqlCommand("DELETE FROM tbl_cart_item WHERE customer_id = @cid", conn, transaction);
        //            clearCart.Parameters.AddWithValue("@cid", customerId);
        //            clearCart.ExecuteNonQuery();

        //            transaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            throw;
        //        }
        //        finally
        //        {
        //            transaction.Commit();
        //            conn.Close();
        //        }
        //    }
        //}

        public IEnumerable<tbl_state> GetAllStates()
        {
            var state_list = new List<tbl_state>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM customers.tbl_state";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    state_list.Add(new tbl_state
                    {
                        StateId = Convert.ToInt32(rd["StateId"]),
                        StateName = rd["StateName"].ToString()
                    });
                }
                conn.Close();
            }
            return state_list;
        }

        public IEnumerable<tbl_district> GetDistrictByStateId(int stateId)
        {
            var district_list = new List<tbl_district>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT DistrictId, DisrictName FROM customers.tbl_district WHERE StateId = @StateId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StateId", stateId);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    district_list.Add(new tbl_district
                    {
                        DistrictId = Convert.ToInt32(rd["DistrictId"]),
                        DisrictName = rd["DisrictName"].ToString()
                    });
                }
                conn.Close();
            }
            return district_list;
        }

        public IEnumerable<tbl_city> GetCitiesByDistrictId(int districtId)
        {
            var cities_list = new List<tbl_city>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT CityId,CityName FROM customers.tbl_city WHERE DistrictId = @DistrictId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@DistrictId", districtId);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    cities_list.Add(new tbl_city
                    {
                        CityId = Convert.ToInt32(rd["CityId"]),
                        CityName = rd["CityName"].ToString()
                    });
                }
                conn.Close();
            }
            return cities_list;
        }

        public void AddAddress(tbl_address tbl_Address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO customers.tbl_address 
        (customer_id, addresses_type, CountryName, StateId, DistrictId, CityId, latitude, longitude, created_at, updated_at, address, door_no, area, landmark) 
        VALUES (@customer_id, @addresses_type, @CountryName, @StateId, @DistrictId, @CityId, @latitude, @longitude, @created_at, @updated_at, @address, @door_no, @area, @landmark)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customer_id", tbl_Address.customer_id);
                cmd.Parameters.AddWithValue("@addresses_type", tbl_Address.addresses_type ?? (object)DBNull.Value); // MATCH PARAMETER NAME
                cmd.Parameters.AddWithValue("@CountryName", tbl_Address.CountryName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@StateId", tbl_Address.StateId);
                cmd.Parameters.AddWithValue("@DistrictId", tbl_Address.DistrictId);
                cmd.Parameters.AddWithValue("@CityId", tbl_Address.CityId);
                cmd.Parameters.AddWithValue("@latitude", tbl_Address.latitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@longitude", tbl_Address.longitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_at", DateTime.Now);
                cmd.Parameters.AddWithValue("@address", tbl_Address.address);
                cmd.Parameters.AddWithValue("@door_no", tbl_Address.door_no);
                cmd.Parameters.AddWithValue("@area", tbl_Address.area);
                cmd.Parameters.AddWithValue("@landmark", tbl_Address.landmark);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public tbl_menu_items GetMenuItemById(int id)
        {
            tbl_menu_items menuItem = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_menu_items WHERE menu_id = @menuId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@menuId", id);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    menuItem = new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        menu_name = rd["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(rd["cuisine_id"]),
                        menu_img = (byte[])rd["menu_img"],
                        menu_descripation = rd["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(rd["amount"]),
                        isAvailable = Convert.ToBoolean(rd["isAvalable"]),
                        Restaurant_id = Convert.ToInt32(rd["Restaurant_id"])
                    };
                }
                conn.Close();
            }
            return menuItem;
        }

        public tbl_customer GetCustomerNameAndPhone(int customerId)
        {
            tbl_customer customer = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring)) 
            {
                string query = @"SELECT customer_name,phone FROM customers.tbl_customer WHERE customer_id=@customerid";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customerid", customerId);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    customer = new tbl_customer
                    {
                        customer_id = customerId,
                        customer_name = rd["customer_name"].ToString(),
                        phone = rd["phone"].ToString()
                    };
                }
                conn.Close();
            }
            return customer;
        }

        public void IncreaseQuantity(int cartItemId)
        {
            UpdateQuantity(cartItemId, 1);
        }

        public void DecreaseQuantity(int cartItemId)
        {
            UpdateQuantity(cartItemId, -1);
        }

        public void RemoveCartItem(int cartItemId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"DELETE FROM customers.tbl_cart_item WHERE cart_item_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", cartItemId);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public decimal GetCartTotal(int customerId)
        {
            decimal total = 0;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT SUM(ci.quantity * mi.amount) AS Total
                    FROM customers.tbl_cart_item ci
                    INNER JOIN vendores.tbl_menu_items mi ON ci.menu_id = mi.menu_id
				    INNER JOIN customers.tbl_cart cc ON cc.cart_id = ci.cart_id
				    where cc.customer_id = @customerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
            return total;
        }

        private void UpdateQuantity(int cartItemId, int delta)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                //Get Current Quantity
                string selectQuery = @"SELECT quantity FROM customers.tbl_cart_item WHERE cart_item_id = @id";
                SqlCommand cmd = new SqlCommand(selectQuery, conn);
                cmd.Parameters.AddWithValue("@id", cartItemId);

                int currentQty = Convert.ToInt32(cmd.ExecuteScalar());

                int newQty = currentQty + delta;

                if(newQty <= 0)
                {
                    //Delete Item form Cart
                    string deleteQuery = @"DELETE FROM customers.tbl_cart_|item where cart_item_id=@id";
                    SqlCommand deletecmd = new SqlCommand(deleteQuery, conn);
                    deletecmd.Parameters.AddWithValue("@id", cartItemId);
                    deletecmd.ExecuteNonQuery();
                }
                else
                {
                    //update quantity
                    string updateQty = "UPDATE customers.tbl_cart_item SET quantity = @qty WHERE cart_item_id = @id";
                    SqlCommand updatecmd = new SqlCommand(updateQty, conn);
                    updatecmd.Parameters.AddWithValue("@id", cartItemId);
                    updatecmd.Parameters.AddWithValue("@qty", newQty);
                    updatecmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public List<tbl_coupone> GetAllCoupons()
        {
            List<tbl_coupone> allcouponse = new List<tbl_coupone>();

            using (SqlConnection conn = new SqlConnection(_connectionstring)) 
            {
                SqlCommand cmd = new SqlCommand("GetAllCoupons", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read()) 
                {
                    allcouponse.Add(new tbl_coupone
                    {
                        coupone_id = Convert.ToInt32(rd["coupone_id"]),
                        coupone_code = rd["coupone_code"].ToString(),
                        title = rd["title"].ToString(),
                        description = rd["description"].ToString(),
                        discount = Convert.ToDecimal(rd["discount"]),
                        application_order_amount = Convert.ToDecimal(rd["application_order_amount"]),
                        expiry_date = Convert.ToDateTime(rd["expiry_date"])
                    });
                }
                conn.Close();
            }
            return allcouponse;
        }

        public tbl_coupone GetAutoApplicableCoupon(decimal grandTotal)
        {
            tbl_coupone coupon = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT TOP 1 * FROM customers.tbl_coupone
                         WHERE application_order_amount <= @grandTotal
                         AND expiry_date > GETDATE()
                         ORDER BY discount DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@grandTotal", grandTotal);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    coupon = new tbl_coupone
                    {
                        coupone_id = Convert.ToInt32(rd["coupone_id"]),
                        coupone_code = rd["coupone_code"].ToString(),
                        discount = Convert.ToDecimal(rd["discount"]),
                        application_order_amount = Convert.ToDecimal(rd["application_order_amount"])
                    };
                }
            }
            return coupon;
        }

        public decimal CalculateGrandTotal(int customer_id)
        {
            decimal total = 0;

            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                con.Open();

                string query = @"SELECT amount, quantity , ci.quantity
FROM customers.tbl_cart_item ci
INNER JOIN vendores.tbl_menu_items mi ON ci.menu_id = mi.menu_id
INNER JOIN customers.tbl_coupone cc ON cc.coupone_id = ci.coupone_id
WHERE ci.cart_id = (SELECT cart_id FROM customers.tbl_cart WHERE customer_id = @CustomerId)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerId", customer_id);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    decimal price = (decimal)reader["list_price"];
                    int qty = (int)reader["quantity"];
                    decimal itemDiscount = (decimal)reader["discount"];

                    decimal subtotal = price * qty;
                    decimal discounted = subtotal - (subtotal * itemDiscount / 100);

                    total += discounted;
                }
            }
            return total;
        }

        public int PlaceOrder(int customer_id, int? coupone_id, int address_id, int paymentModeId, decimal totalAmount, decimal discount, string razorpayPaymentId, string razorpayOrderId)
        {
            int orderId = 0;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO customers.tbl_orders 
                            (customer_id, order_date, order_status, coupone_id, deliver_dateTime, food_status, 
                             Payment_mode_id, addressid, resturant_id, razorpay_payment_id, razorpay_order_id)
                             OUTPUT INSERTED.order_id
                             VALUES (@customerId, GETDATE(), 'Pending', @couponeId, DATEADD(HOUR, 1, GETDATE()), 'Processing', 
                                     @paymentModeId, @addressId, 1, @razorpayPaymentId, @razorpayOrderId)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@customerId", customer_id);
                    cmd.Parameters.AddWithValue("@couponId", coupone_id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@paymentModeId", paymentModeId);
                    cmd.Parameters.AddWithValue("@addressId", address_id);
                    cmd.Parameters.AddWithValue("@razorpayPaymentId", razorpayPaymentId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@razorpayOrderId", razorpayOrderId ?? (object)DBNull.Value);

                    conn.Open();
                    orderId = (int)cmd.ExecuteScalar();
                }
            }
            return orderId;
        }

        public void SaveOrderItem(int orderId, int menuId, int quantity, decimal price, decimal discount)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO customers.tbl_order_items (order_id, menu_id, quantity, list_price, discount, estimated_DATETIME)
                             VALUES (@orderId, @menuId, @quantity, @price, @discount, DATEADD(HOUR, 1, GETDATE()))";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@menuId", menuId);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@discount", discount);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<tbl_special_offers> GetAllActiveOffers()
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_special_offers WHERE is_Active = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    string imageName = rd["image_path"].ToString();
                    string imagePath = imageName;

                    offers.Add(new tbl_special_offers
                    {
                        so_id = (int)rd["so_id"],
                        restaurant_id = (int)rd["restaurant_id"],
                        offer_title = rd["offer_title"].ToString(),
                        offer_desc = rd["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(rd["percentage_disc"]),
                        validFrom = Convert.ToDateTime(rd["validFrom"]),
                        validTo = Convert.ToDateTime(rd["validTo"]),
                        is_Active = Convert.ToBoolean(rd["is_Active"]),
                        menu_id = (int)rd["menu_id"],
                        ImagePath = imagePath
                    });
                }
                conn.Close();
            }
            return offers;
        }

        public IEnumerable<tbl_special_offers> GetOffers()
        {
            var offers = new List<tbl_special_offers>();

            using (var con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_special_offers";
                SqlCommand cmd = new SqlCommand(query, con);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    offers.Add(new tbl_special_offers
                    {
                        offer_title = reader["offer_title"].ToString(),
                        offer_desc = reader["offer_desc"].ToString(),
                        ImagePath = reader["image_path"].ToString(),
                        percentage_disc = Convert.ToInt32(reader["percentage_disc"]),
                        validFrom = Convert.ToDateTime(reader["validFrom"]),
                        validTo = Convert.ToDateTime(reader["validTo"]),
                        is_Active = Convert.ToBoolean(reader["is_Active"]),
                        menu_id = (int)reader["menu_id"],
                    });
                }
            }

            return offers;
        }

        public tbl_special_offers GetOfferById(int offerId)
        {
            tbl_special_offers offer = null;

            using (var con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT * FROM vendores.tbl_special_offers WHERE so_id = @OfferId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@OfferId", offerId);
                
                con.Open();

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    offer = new tbl_special_offers
                    {
                        so_id = Convert.ToInt32(reader["so_id"]),
                        offer_title = reader["offer_title"].ToString(),
                        offer_desc = reader["offer_desc"].ToString(),
                        ImagePath = reader["image_path"].ToString(),
                        percentage_disc = Convert.ToInt32(reader["percentage_disc"]),
                        validFrom = Convert.ToDateTime(reader["validFrom"]),
                        validTo = Convert.ToDateTime(reader["validTo"]),
                        is_Active = Convert.ToBoolean(reader["is_Active"]),
                        menu_id = (int)reader["menu_id"],
                    };
                }
            }

            return offer;
        }

        public List<tbl_cuisine_master> GetCuisinesByRestaurantId(int restaurantId)
        {
            var cuisines = new List<tbl_cuisine_master>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT cm.cuisine_id, cm.cuisine_name 
                                FROM admins.tbl_cuisine_master cm
                                INNER JOIN vendores.tbl_cuisine c ON cm.cuisine_id = c.cuisine_id
                                 WHERE c.Restaurnat_id = @restaurantId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@restaurantId", restaurantId);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    cuisines.Add(new tbl_cuisine_master
                    {
                        cuisine_id = Convert.ToInt32(rd["cuisine_id"]),
                        cuisine_name = rd["cuisine_name"].ToString()
                    });
                }
                conn.Close();
            }
            return cuisines;
        }

        public RestaurantMenuViewModel GetRestaurantMenu(int restaurantId, int? cuisineId)
        {
            var model = new RestaurantMenuViewModel();
            model.restauranr_id = restaurantId;
            model.Cuisines = new List<tbl_cuisine_master>();
            model.MenuItems = new List<MenuItemViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                // Get Restaurant Name
                using (SqlCommand cmd = new SqlCommand("SELECT restaurant_name FROM vendedores.tbl_restaurant WHERE restaurant_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", restaurantId);
                    model.restaurant_name = cmd.ExecuteScalar()?.ToString();
                }

                // Get All Cuisines for the restaurant
                using (SqlCommand cmd = new SqlCommand("SELECT c.cuisine_id, cm.cuisine_name FROM vendedores.tbl_cuisine c JOIN admins.tbl_cuisine_master cm ON c.cuisine_id = cm.cuisine_id WHERE c.Restaurant_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", restaurantId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Cuisines.Add(new tbl_cuisine_master
                            {
                                cuisine_id = Convert.ToInt32(reader["cuisine_id"]),
                                cuisine_name = reader["cuisine_name"].ToString()
                            });
                        }
                    }
                }

                // Get Menu Items based on selected cuisine
                string menuQuery = @"SELECT menu_id, menu_name, menu_img, menu_description, amount, cuisine_id 
                                 FROM vendedores.tbl_menu_items 
                                 WHERE Restaurant_id = @id";

                if (cuisineId.HasValue)
                {
                    menuQuery += " AND cuisine_id = @cuisineId";
                }

                using (SqlCommand cmd = new SqlCommand(menuQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@id", restaurantId);
                    if (cuisineId.HasValue)
                        cmd.Parameters.AddWithValue("@cuisineId", cuisineId.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.MenuItems.Add(new MenuItemViewModel
                            {
                                MenuId = Convert.ToInt32(reader["menu_id"]),
                                MenuName = reader["menu_name"].ToString(),
                                MenuImg = reader["menu_img"] as byte[],
                                MenuDescription = reader["menu_description"].ToString(),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                cuisine_id = Convert.ToInt32(reader["cuisine_id"])
                            });
                        }
                    }
                }

                model.SelectedCuisineId = cuisineId;
            }

            return model;
        }

        public List<RestaurantCardViewModel> getAppovedOnlineRestaurants()
        {
            List<RestaurantCardViewModel> restaurantList = new List<RestaurantCardViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT  rs.restaurant_id,
                                rs.restaurant_name,
                                rs.restaurant_street + ', ' + rs.restaurant_pincode AS full_address,
                                vi.Restaurant_img FROM vendores.tbl_restaurant rs
                                INNER JOIN vendores.tbl_vendores_img vi ON rs.restaurant_id = vi.Restaurant_id";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    restaurantList.Add(new RestaurantCardViewModel
                    {
                        restaurant_id = Convert.ToInt32(reader["restaurant_id"]),
                        restaurant_name = reader["restaurant_name"].ToString(),
                        full_address = reader["full_address"].ToString(),
                        restaurantImage = (byte[])reader["Restaurant_img"]
                    });
                }
            }
            return restaurantList;
        }

        public RestaurantMenuViewModel getRestaurantDetails(int restaurantId)
        {
            RestaurantMenuViewModel restaurant = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT rs.restaurant_id, rs.restaurant_name, cm.cuisine_name,
                                   rs.restaurant_street + ' ' + rs.restaurant_pincode AS address,
                                   rs.restaurant_contact,
                                   FORMAT(va.open_DATETIME, 'hh\\:mm tt') AS open_time,
                                   FORMAT(va.close_DATETIME, 'hh\\:mm tt') AS close_time,
                                   vi.Restaurant_img,
                                   vi.Restaurant_menu_img
                            FROM vendores.tbl_restaurant rs
                            INNER JOIN vendores.tbl_cuisine vc ON vc.Restaurnat_id = rs.restaurant_id
                            INNER JOIN admins.tbl_cuisine_master cm ON cm.cuisine_id = vc.cuisine_id
                            INNER JOIN vendores.tbl_vendor_availability va ON va.Restaurant_id = rs.restaurant_id
                            INNER JOIN vendores.tbl_vendores_img vi ON vi.Restaurant_id = rs.restaurant_id
                            WHERE rs.restaurant_id = @RestaurantId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    restaurant = new RestaurantMenuViewModel
                    {
                        restauranr_id = Convert.ToInt32(rd["restaurant_id"]),
                        restaurant_name = rd["restaurant_name"].ToString(),
                        cuisine_name = rd["cuisine_name"].ToString(),
                        restaurant_street = rd["restaurant_street"].ToString(),
                        restaurant_pincode = rd["restaurant_pincode"].ToString(),
                        restaurant_contact = rd["restaurant_contact"].ToString(),
                        open_DATETIME = rd["open_DATETIME"].ToString(),
                        close_DATETIME = rd["close_DATETIME"].ToString(),
                        Restaurant_images = (byte[])rd["Restaurant_images"],
                        Menu_Image = (byte[])rd["Menu_Image"]
                    };
                }
                conn.Close();
            }
            return restaurant;
        }

        public IEnumerable<tbl_menu_items> GetAllPhotos()
        {
            List<tbl_menu_items> photos = new List<tbl_menu_items>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendors.tbl_menu_items";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    photos.Add(new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(reader["menu_id"]),
                        menu_name = reader["menu_name"].ToString(),
                        menu_img = (byte[])reader["menu_img"],
                        menu_descripation = reader["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(reader["amount"]),
                        isAvailable = Convert.ToBoolean(reader["isAvalable"]),
                        Restaurant_id = Convert.ToInt32(reader["Restaurant_id"])
                    });
                }
                conn.Close();
            }
            return photos;
        }

        public IEnumerable<tbl_menu_items> GetMenuItemsByRestaurant(int restaurantId)
        {
            List<tbl_menu_items> menuItems = new List<tbl_menu_items>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_menu_items WHERE Restaurant_id = @Restaurant_id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Restaurant_id", restaurantId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    menuItems.Add(new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(reader["menu_id"]),
                        menu_name = reader["menu_name"].ToString(),
                        menu_img = reader["menu_img"] as byte[],
                        menu_descripation = reader["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(reader["amount"]),
                        isAvailable = Convert.ToBoolean(reader["isAvalable"]),
                        Restaurant_id = Convert.ToInt32(reader["Restaurant_id"])
                    });
                }

                conn.Close();
            }

            return menuItems;
        }

        //public RestaurantDetailsViewModel getRestaurantDetailsByresId(int restaurantId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
