using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Foodie.Models.customers.tbl_coupone;
using System.Data;
using System;
using Foodie.Models.Admin;
using Foodie.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Configuration;
using Foodie.ViewModels;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Razorpay.Api;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

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
            using (SqlConnection conn = new SqlConnection(_connectionstring))
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
            using (SqlConnection conn = new SqlConnection(_connectionstring))
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
            using (SqlConnection conn = new SqlConnection(_connectionstring))
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

                if (newQty <= 0)
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

        //public int PlaceOrder(int customer_id, int? coupone_id, int address_id, int paymentModeId, decimal totalAmount, decimal discount, string razorpayPaymentId, string razorpayOrderId)
        //{
        //    int orderId = 0;

        //    using (SqlConnection conn = new SqlConnection(_connectionstring))
        //    {
        //        string query = @"INSERT INTO customers.tbl_orders 
        //                    (customer_id, order_date, order_status, coupone_id, deliver_dateTime, food_status, 
        //                     Payment_mode_id, addressid, resturant_id, razorpay_payment_id, razorpay_order_id)
        //                     OUTPUT INSERTED.order_id
        //                     VALUES (@customerId, GETDATE(), 'Pending', @couponeId, DATEADD(HOUR, 1, GETDATE()), 'Processing', 
        //                             @paymentModeId, @addressId, 1, @razorpayPaymentId, @razorpayOrderId)";
        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@customerId", customer_id);
        //            cmd.Parameters.AddWithValue("@couponId", coupone_id ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@paymentModeId", paymentModeId);
        //            cmd.Parameters.AddWithValue("@addressId", address_id);
        //            cmd.Parameters.AddWithValue("@razorpayPaymentId", razorpayPaymentId ?? (object)DBNull.Value);
        //            cmd.Parameters.AddWithValue("@razorpayOrderId", razorpayOrderId ?? (object)DBNull.Value);

        //            conn.Open();
        //            orderId = (int)cmd.ExecuteScalar();
        //        }
        //    }
        //    return orderId;
        //}

        //public void SaveOrderItem(int orderId, int menuId, int quantity, decimal price, decimal discount)
        //{
        //    using (SqlConnection con = new SqlConnection(_connectionstring))
        //    {
        //        string query = @"INSERT INTO customers.tbl_order_items (order_id, menu_id, quantity, list_price, discount, estimated_DATETIME)
        //                     VALUES (@orderId, @menuId, @quantity, @price, @discount, DATEADD(HOUR, 1, GETDATE()))";

        //        using (SqlCommand cmd = new SqlCommand(query, con))
        //        {
        //            cmd.Parameters.AddWithValue("@orderId", orderId);
        //            cmd.Parameters.AddWithValue("@menuId", menuId);
        //            cmd.Parameters.AddWithValue("@quantity", quantity);
        //            cmd.Parameters.AddWithValue("@price", price);
        //            cmd.Parameters.AddWithValue("@discount", discount);

        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        public IEnumerable<tbl_special_offers> GetAllActiveOffers()
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
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
            model.restaurant_id = restaurantId;
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
                        restaurant_id = Convert.ToInt32(rd["restaurant_id"]),
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

        public List<MenuItemViewModel> GetInspirationItems()
        {
            var menuItems = new List<MenuItemViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT TOP 10 menu_id,menu_name, menu_img FROM vendores.tbl_menu_items WHERE isAvalable = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string menuName = reader["menu_name"].ToString();

                    string base64Image = string.Empty;

                    if (reader["menu_img"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])reader["menu_img"];
                        base64Image = Convert.ToBase64String(imgBytes);

                    }
                    menuItems.Add(new MenuItemViewModel
                    {
                        MenuId = Convert.ToInt32(reader["menu_id"]),
                        MenuName = reader["menu_name"].ToString(),
                        MenuImageBase64 = base64Image
                    });
                }
                conn.Close();
            }
            return menuItems;
        }

        public MenuItemViewModel GetInspireMenuItemById(int menuid)
        {
            MenuItemViewModel menuItem = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_menu_items WHERE menu_id = @menuId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@menuId", menuid);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    string menuName = rd["menu_name"].ToString();
                    string base64Image = string.Empty;
                    if (rd["menu_img"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])rd["menu_img"];
                        base64Image = Convert.ToBase64String(imgBytes);
                    }
                    menuItem = new MenuItemViewModel
                    {
                        MenuId = Convert.ToInt32(rd["menu_id"]),
                        MenuName = menuName,
                        MenuImageBase64 = base64Image,
                        Amount = Convert.ToDecimal(rd["amount"]),
                        MenuDescription = rd["menu_descripation"].ToString()
                    };
                }
                conn.Close();
            }
            return menuItem;
        }

        public List<AvailableDishViewModel> GetDeliveryFoods(int menuId)
        {
            List<AvailableDishViewModel> list = new List<AvailableDishViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT  mi.menu_id,
                vi.Restaurant_menu_img,
                rs.restaurant_name,
                cm.cuisine_name,
                mi.amount,
                mi.isAvalable,
                mi.menu_name,
                mi.menu_descripation
            FROM vendores.tbl_restaurant as rs
            INNER JOIN vendores.tbl_vendores_img vi ON rs.restaurant_id = vi.Restaurant_id
            INNER JOIN vendores.tbl_cuisine tc ON rs.restaurant_id = tc.Restaurnat_id
            INNER JOIN admins.tbl_cuisine_master cm ON tc.cuisine_id = cm.cuisine_id
            INNER JOIN vendores.tbl_menu_items mi ON mi.Restaurant_id = rs.restaurant_id
            WHERE mi.isAvalable = 1 AND mi.menu_id = @menuId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@menuId", menuId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string base64Image = string.Empty;

                    if (reader["Restaurant_menu_img"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])reader["Restaurant_menu_img"];
                        base64Image = Convert.ToBase64String(imgBytes);
                    }

                    list.Add(new AvailableDishViewModel
                    {
                        MenuId = Convert.ToInt32(reader["menu_id"]),
                        RestaurantImagebase64 = base64Image,
                        RestaurantName = reader["restaurant_name"].ToString(),
                        CuisineName = reader["cuisine_name"].ToString(),
                        Amount = Convert.ToDecimal(reader["amount"]),
                        IsAvailable = Convert.ToBoolean(reader["isAvalable"]),
                        MenuName = reader["menu_name"].ToString(),
                        Menudescription = reader["menu_descripation"].ToString()
                    });
                }
                conn.Close();
            }
            return list;
        }

        public int PlaceOrder(RazorPayViewModel model)
        {
            int orderId;
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO customers.tbl_orders (customer_id, order_date, order_status, coupone_id, deliver_dateTime, food_status, Payment_mode_id, addressid, resturant_id) " +
                    "OUTPUT INSERTED.order_id VALUES (@customer_id, GETDATE(), 'Pending', @coupone_id, GETDATE(), 'Processing', @paymentModeId, @addressid, @restaurantId)", conn);

                cmd.Parameters.AddWithValue("@customer_id", model.customer_id);
                cmd.Parameters.AddWithValue("@coupone_id", (object?)model.coupone_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@paymentModeId", model.PaymentModeId);
                cmd.Parameters.AddWithValue("@addressid", model.AddressId);
                cmd.Parameters.AddWithValue("@restaurantId", model.restaurant_id);

                orderId = (int)cmd.ExecuteScalar();
            }

            return orderId;
        }

        public void SaveOrderItem(int orderId, OrderItemModel item)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO customers.tbl_order_items (order_id, menu_id, quantity, list_price, discount, estimated_DATETIME) " +
                    "VALUES (@order_id, @menu_id, @quantity, @list_price, @discount, @estimated_DATETIME)", conn);

                cmd.Parameters.AddWithValue("@order_id", orderId);
                cmd.Parameters.AddWithValue("@menu_id", item.menu_id);
                cmd.Parameters.AddWithValue("@quantity", item.quantity);
                cmd.Parameters.AddWithValue("@list_price", item.listprice);
                cmd.Parameters.AddWithValue("@discount", item.discount);
                cmd.Parameters.AddWithValue("@estimated_DATETIME", item.EstimatedTime);

                cmd.ExecuteNonQuery();
            }
        }
        public tbl_orders CreateOrder(int customerId, decimal grandTotal, string razorpayOrderId, List<tbl_order_items> items, int addressId, int res_id)
        {
            using (var connection = new SqlConnection(_connectionstring))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    var orderQuery = @"INSERT INTO customers.tbl_orders (customer_id, order_date, order_status, grand_total, razorpay_order_id, resturant_id, addressid)
                                       OUTPUT INSERTED.order_id
                                       VALUES (@CustomerId, GETDATE(), 'Pending', @GrandTotal, @RazorpayOrderId, @restaurantId, @AddressId)";
                    var orderCmd = new SqlCommand(orderQuery, connection, transaction);
                    orderCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    orderCmd.Parameters.AddWithValue("@GrandTotal", grandTotal);
                    orderCmd.Parameters.AddWithValue("@RazorpayOrderId", razorpayOrderId);
                    orderCmd.Parameters.AddWithValue("@restaurantId", res_id);
                    orderCmd.Parameters.AddWithValue("@AddressId", addressId);
                    var orderId = (int)orderCmd.ExecuteScalar();

                    foreach (var item in items)
                    {
                        var itemQuery = @"INSERT INTO customers.tbl_order_items (order_id, menu_id, quantity, list_price, discount, estimated_DATETIME)
                                          VALUES (@OrderId, @MenuId, @Quantity, @ListPrice, @Discount, @EstimatedTime)";
                        var itemCmd = new SqlCommand(itemQuery, connection, transaction);
                        itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                        itemCmd.Parameters.AddWithValue("@MenuId", item.menu_id);
                        itemCmd.Parameters.AddWithValue("@Quantity", item.quantity);
                        itemCmd.Parameters.AddWithValue("@ListPrice", item.list_price);
                        itemCmd.Parameters.AddWithValue("@Discount", item.discount);
                        itemCmd.Parameters.AddWithValue("@EstimatedTime", DateTime.Now.AddHours(1));
                        itemCmd.ExecuteNonQuery();
                    }

                    // After successfully creating order and items, clear the cart
                    string clearCartSql = @"
                    DELETE ci 
                    FROM customers.tbl_cart_item ci
                    JOIN customers.tbl_cart c ON ci.cart_id = c.cart_id
                    WHERE c.customer_id = @CustomerId";

                        SqlCommand clearCartCmd = new SqlCommand(clearCartSql, connection, transaction);
                        clearCartCmd.Parameters.AddWithValue("@CustomerId", customerId);
                        clearCartCmd.ExecuteNonQuery();

                        transaction.Commit();
                    //transaction.Commit();

                    return new tbl_orders
                    {
                        order_id = orderId,
                        customer_id = customerId,
                        grand_total = grandTotal,
                        razorpay_order_id = razorpayOrderId,
                        order_status = "Pending",
                        OrderItems = items
                    };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateOrderStatus(string razorpayOrderId, string status)
        {
            using (var conn = new SqlConnection(_connectionstring))
            {
                var query = "UPDATE customers.tbl_orders SET order_status = @OrderStatus WHERE razorpay_order_id = @RazorpayOrderId";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RazorpayOrderId", razorpayOrderId);
                cmd.Parameters.AddWithValue("@OrderStatus", status);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SavePayment(string razorpayOrderId, string razorpayPaymentId, decimal amount, string status)
        {
            using (var conn = new SqlConnection(_connectionstring))
            {
                var query = @"INSERT INTO customers.payments (order_id, razorpay_payment_id, amount, payment_status, payment_date)
                              SELECT order_id, @RazorpayPaymentId, @Amount, @Status, GETDATE()
                              FROM customers.tbl_orders WHERE razorpay_order_id = @RazorpayOrderId";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RazorpayOrderId", razorpayOrderId);
                cmd.Parameters.AddWithValue("@RazorpayPaymentId", razorpayPaymentId);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Status", status);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool UpdateCartItemQuantity(int cartItemId, int newQuantity)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string sql = "UPDATE customers.tbl_cart_item SET quantity = @Quantity WHERE cart_item_id = @CartItemId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                cmd.Parameters.AddWithValue("@CartItemId", cartItemId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public tbl_cart_item GetCartItemById(int cartItemId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string sql = "SELECT * FROM customers.tbl_cart_item WHERE cart_item_id = @CartItemId";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CartItemId", cartItemId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new tbl_cart_item
                        {
                            cart_item_id = Convert.ToInt32(reader["cart_item_id"]),
                            cart_id = Convert.ToInt32(reader["cart_id"]),
                            menu_id = Convert.ToInt32(reader["menu_id"]),
                            quantity = Convert.ToInt32(reader["quantity"]),
                            price = Convert.ToDecimal(reader["price"]),
                            coupone_id = Convert.ToInt32(reader["coupone_id"])
                        };
                    }
                }
            }
            return null;
        }

        public CustomerFeedbackViewModel GetCustomerFeedbacks(int pageNumber, int pageSize, string sortField, string sortDirection)
        {
            var feedbacks = new List<tbl_customer_feedback>();

            int totalCount = 0;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                var valiSortFields = new[] { "feedback_id", "customer_id", "order_id", "rating", "comment", "created_at" };

                sortField = valiSortFields.Contains(sortField) ? sortField : "created_at";
                sortDirection = sortDirection.ToLower() == "asc" ? "ASC" : "DESC";

                // Get paginated data
                string query = $@"SELECT cust_feedback_id, cust_feedback_id, restaurant_id, rating, feedback_description, createdAt
                FROM admins.tbl_customer_feedback
                ORDER BY {sortField} {sortDirection}
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    feedbacks.Add(new tbl_customer_feedback
                    {
                        cust_feedback_id = Convert.ToInt32(reader["cust_feedback_id"]),
                        restaurant_id = Convert.ToInt32(reader["restaurant_id"]),
                        rating = Convert.ToInt32(reader["rating"]),
                        feedback_description = reader["feedback_description"].ToString(),
                        createdAt = Convert.ToDateTime(reader["createdAt"])
                    });
                }
            }

            return new CustomerFeedbackViewModel
            {
                RecentFeedbacks = feedbacks,
                TotalPages = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                SortField = sortField,
                SortDirection = sortDirection
            };
        }

        public int GetTotalFeedbackCountAsync()
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT COUNT(*) FROM admins.tbl_customer_feedback";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                conn.Close();
                return count;
            }
        }

        public void InsertFeedback(tbl_customer_feedback feedback)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO admins.tbl_customer_feedback (customer_id, restaurant_id, rating, feedback_description,createdAt) VALUES (@CustomerId, @RestaurantId, @Rating, @Description,@createdAt)", con);
                cmd.Parameters.AddWithValue("@CustomerId", feedback.customer_id);
                cmd.Parameters.AddWithValue("@RestaurantId", feedback.restaurant_id);
                cmd.Parameters.AddWithValue("@Rating", feedback.rating);
                cmd.Parameters.AddWithValue("@Description", feedback.feedback_description);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<tbl_restaurant> GetApprovedRestaurants()
        {
            List<tbl_restaurant> restaurants = new List<tbl_restaurant>();

            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT restaurant_id, restaurant_name FROM vendores.tbl_restaurant WHERE restaurant_isApprov = 1";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    restaurants.Add(new tbl_restaurant
                    {
                        restaurant_id = Convert.ToInt32(dr["restaurant_id"]),
                        restaurant_name = dr["restaurant_name"].ToString()
                    });
                }
            }

            return restaurants;
        }

        public IEnumerable<tbl_cuisine_master> GetCuisinesByRestaurant(int restaurantId)
        {
            List<tbl_cuisine_master> cuisines = new List<tbl_cuisine_master>();

            using (SqlConnection connection = new SqlConnection(_connectionstring))
            {
                connection.Open();

                string query = @"
                SELECT 
                    vc.cuisine_id,
                    ac.cuisine_name,
                    COUNT(mi.menu_id) AS item_count
                FROM 
                    admins.tbl_cuisine_master ac
                INNER JOIN 
                    vendores.tbl_cuisine vc ON vc.cuisine_id = ac.cuisine_id
                INNER JOIN
                    vendores.tbl_restaurant_cuisine rc ON rc.cuisine_id = vc.cuisine_id
                LEFT JOIN 
                    vendores.tbl_menu_items mi ON mi.cuisine_id = vc.cuisine_id AND mi.restaurant_id = @RestaurantId
                WHERE
                    rc.restaurant_id = @RestaurantId
                GROUP BY 
                    vc.cuisine_id, ac.cuisine_name
                ORDER BY 
                    ac.cuisine_name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RestaurantId", restaurantId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cuisines.Add(new tbl_cuisine_master
                            {
                                cuisine_id = reader.GetInt32(0),
                                cuisine_name = reader.GetString(1),
                                item_count = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }

            return cuisines;
        }

        public IEnumerable<MenuItemViewModel> GetMenuItemsByRestaurantAndCuisine(int restaurantId, int cuisineId)
        {
            List<MenuItemViewModel> menuItems = new List<MenuItemViewModel>();

            using (SqlConnection connection = new SqlConnection(_connectionstring))
            {
                connection.Open();

                string query = @"
                SELECT 
                    mi.menu_id,
                    mi.menu_name,
                    mi.menu_descripation,
                    mi.amount,
                    mi.menu_img,
                    vc.cuisine_id,
                    ac.cuisine_name
                FROM 
                    vendores.tbl_menu_items mi
                INNER JOIN 
                    vendores.tbl_cuisine vc ON mi.cuisine_id = vc.cuisine_id
                INNER JOIN 
                    admins.tbl_cuisine_master ac ON vc.cuisine_id = ac.cuisine_id
                WHERE 
                    mi.restaurant_id = @RestaurantId
                    AND mi.cuisine_id = @CuisineId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                    command.Parameters.AddWithValue("@CuisineId", cuisineId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuItems.Add(new MenuItemViewModel
                            {
                                MenuId = reader.GetInt32(0),
                                MenuName = reader.GetString(1),
                                MenuDescription = reader.GetString(2),
                                Amount = reader.GetDecimal(3),
                                MenuImg = reader.IsDBNull(4) ? null : (byte[])reader[4],
                                cuisine_id = reader.GetInt32(5),
                                cuisine_name = reader.GetString(6)
                            });
                        }
                    }
                }
            }
            return menuItems;
        }


        public RestaurantInfo GetRestaurantByName(string restaurantName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT  
                        rs.restaurant_id,
                        Restaurant_name,
                        restaurant_img,
                        rs.restaurant_street + ' ' + rs.restaurant_pincode AS [address],
                        rs.restaurant_contact,
                        rs.restaurant_email,
                        va.day_of_week,
                        va.open_DATETIME,
                        va.close_DATETIME
                    FROM vendores.tbl_restaurant rs
                    INNER JOIN vendores.tbl_vendores_img vi ON rs.restaurant_id = vi.Restaurant_id
                    INNER JOIN vendores.tbl_vendor_availability va ON va.Restaurant_id = rs.restaurant_id
                    WHERE 
                        customers.RemoveExtraSpaces(restaurant_name) = customers.RemoveExtraSpaces(@name)
                        AND restaurant_isApprov = 1", conn))
                {
                    string normalizedName = Regex.Replace(restaurantName.Trim(), @"\s+", " ");
                    cmd.Parameters.AddWithValue("@name", normalizedName);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DateTime openTime = reader["open_DATETIME"] != DBNull.Value
                                ? Convert.ToDateTime(reader["open_DATETIME"])
                                : DateTime.MinValue;

                            DateTime closeTime = reader["close_DATETIME"] != DBNull.Value
                                ? Convert.ToDateTime(reader["close_DATETIME"])
                                : DateTime.MinValue;

                            return new RestaurantInfo
                            {
                                RestaurantId = Convert.ToInt32(reader["restaurant_id"]),
                                Restaurant_name = reader["Restaurant_name"].ToString(),
                                Restaurant_img = reader["restaurant_img"] as byte[],
                                Description = string.Empty,
                                Address = reader["address"].ToString(),
                                Phone = reader["restaurant_contact"].ToString(),
                                Email = reader["restaurant_email"].ToString(),
                                Rating = 0,
                                DayOfWeek = reader["day_of_week"] != DBNull.Value
                                    ? reader["day_of_week"].ToString().Split(',').Length
                                    : 0,
                                OpeningTime = openTime != DateTime.MinValue
                                    ? openTime.TimeOfDay
                                    : TimeSpan.Zero,
                                ClosingTime = closeTime != DateTime.MinValue
                                    ? closeTime.TimeOfDay
                                    : TimeSpan.Zero
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error in GetRestaurantByName: {ex.Message}");
            }
            return null;
        }

        public List<RestaurantInfo> GetTopRestaurant(int count = 5)
        {
            var brands = new List<RestaurantInfo>();

            using (var connection = new SqlConnection(_connectionstring))
            {
                var command = new SqlCommand("SELECT TOP (@Count) * FROM vendores.tbl_restaurant rs inner join customers.tbl_rating cr on rs.restaurant_id = cr.restaurant_id WHERE restaurant_isApprov = 1 ORDER BY rating DESC", connection);
                command.Parameters.AddWithValue("@Count", count);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(new RestaurantInfo
                        {
                            RestaurantId = Convert.ToInt32(reader["restaurant_id"]),
                            Restaurant_name = reader["restaurant_name"].ToString(),
                            Restaurant_img = reader["Restaurant_img"] as byte[],
                            Description = reader["Description"].ToString(),
                            Address = reader["Address"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Email = reader["Email"].ToString(),
                            Rating = Convert.ToDecimal(reader["rating"]),
                            DayOfWeek = Convert.ToInt32(reader["DayOfWeek"]),
                            OpeningTime = TimeSpan.Parse(reader["OpeningTime"].ToString()),
                            ClosingTime = TimeSpan.Parse(reader["ClosingTime"].ToString())
                        });
                    }
                }
            }

            return brands;
        }

        List<MenuItemViewModel> IcustomerRepository.GetMenuItemsByRestaurantdetails(int restaurantId)
        {
            var menuItems = new List<MenuItemViewModel>();

            using (var connection = new SqlConnection(_connectionstring))
            {
                var command = new SqlCommand(
                    @"SELECT mi.*, cm.cuisine_name 
              FROM vendores.tbl_menu_items mi
              JOIN admins.tbl_cuisine_master cm ON mi.cuisine_id = cm.cuisine_id
              WHERE mi.Restaurant_id = @RestaurantId AND mi.IsAvalable = 1",
                    connection);

                command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        menuItems.Add(new MenuItemViewModel
                        {
                            MenuId = Convert.ToInt32(reader["menu_id"]),
                            MenuName = reader["menu_name"].ToString(),
                            MenuImg = reader["menu_img"] != DBNull.Value ? (byte[])reader["menu_img"] : null,
                            MenuDescription = reader["menu_descripation"].ToString(),
                            Amount = Convert.ToDecimal(reader["amount"]),
                            cuisine_name = reader["cuisine_name"].ToString(),
                            cuisine_id = Convert.ToInt32(reader["cuisine_id"]),
                            RestaurantId = restaurantId,
                            IsAvalable = Convert.ToBoolean(reader["IsAvalable"]),
                            MenuImageBase64 = reader["menu_img"] != DBNull.Value
                                ? Convert.ToBase64String((byte[])reader["menu_img"])
                                : string.Empty
                        });
                    }
                }
            }

            return menuItems;
        }

        MenuItemViewModel IcustomerRepository.GetMenuViewModelAsync(int restaurantId, int? cuisineId)
        {
            List<MenuItemViewModel> cuisines = new List<MenuItemViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT DISTINCT mi.cuisine_id, cuisine_name FROM admins.tbl_cuisine_master ad inner join vendores.tbl_menu_items mi on ad.cuisine_id = mi.cuisine_id 
                         WHERE Restaurant_id = @restaurantId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@restaurantId", restaurantId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cuisines.Add(new MenuItemViewModel
                            {
                                cuisine_id = Convert.ToInt32(reader["cuisine_id"]),
                                cuisine_name = reader["cuisine_name"].ToString()
                            });
                        }
                    }
                }
            }

            return null;
        }

        public List<MenuItemViewModel> getCuisinrByRestaurant(int restaurantId)
        {
            throw new NotImplementedException();
        }

        public List<tbl_cuisine_master> GetCuisinesWithCountAsync(int restaurantId)
        {
            throw new NotImplementedException();
        }

        public List<SimilarRestaurantDtoViewModel> GetSimilarRestaurants(
            string currentRestaurantName,
            List<string> cuisines,
            List<string> pincodes)
        {
            var similarRestaurants = new List<SimilarRestaurantDtoViewModel>();

            using (var connection = new SqlConnection(_connectionstring))
            {
                var query = new StringBuilder(@"
            SELECT TOP 3 
                rs.restaurant_name, 
                ac.cuisine_name, 
                rs.restaurant_street + ' ' + rs.restaurant_pincode as address, 
                vi.Restaurant_img 
            FROM vendores.tbl_restaurant rs
            INNER JOIN vendores.tbl_cuisine vc ON rs.restaurant_id = vc.Restaurnat_id
            INNER JOIN admins.tbl_cuisine_master ac ON ac.cuisine_id = vc.cuisine_id
            INNER JOIN vendores.tbl_vendores_img vi ON vi.Restaurant_id = rs.restaurant_id
            WHERE rs.restaurant_name != @CurrentRestaurantName 
            AND (");

                // Build cuisine conditions
                if (cuisines.Any())
                {
                    query.Append("(");
                    for (int i = 0; i < cuisines.Count; i++)
                    {
                        if (i > 0) query.Append(" OR ");
                        query.Append($"ac.cuisine_name LIKE @Cuisine{i}");
                    }
                    query.Append(")");
                }

                // Add pincode conditions if both cuisines and pincodes are provided
                if (cuisines.Any() && pincodes.Any())
                {
                    query.Append(" OR ");
                }

                // Build pincode conditions
                if (pincodes.Any())
                {
                    query.Append("(");
                    for (int i = 0; i < pincodes.Count; i++)
                    {
                        if (i > 0) query.Append(" OR ");
                        query.Append($"rs.restaurant_pincode = @Pincode{i}");
                    }
                    query.Append(")");
                }

                query.Append(") ORDER BY NEWID()");

                using (var command = new SqlCommand(query.ToString(), connection))
                {
                    command.Parameters.AddWithValue("@CurrentRestaurantName", currentRestaurantName);

                    // Add cuisine parameters
                    for (int i = 0; i < cuisines.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@Cuisine{i}", $"%{cuisines[i]}%");
                    }

                    // Add pincode parameters
                    for (int i = 0; i < pincodes.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@Pincode{i}", pincodes[i]);
                    }

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            similarRestaurants.Add(new SimilarRestaurantDtoViewModel
                            {
                                restaurant_name = reader["restaurant_name"].ToString(),
                                cuisine_name = reader["cuisine_name"].ToString(),
                                address = reader["address"].ToString(),
                                Restaurant_img = reader["Restaurant_img"] == DBNull.Value
                                    ? "/images/default-restaurant.jpg"
                                    : reader["Restaurant_img"].ToString()
                            });
                        }
                    }
                }
            }

            return similarRestaurants;

        }

        public List<MenuItemViewModel> GetMenuItemsImageByRestaurant(string restaurantName)
        {
            var menuImages = new List<MenuItemViewModel>();

            using (var connection = new SqlConnection(_connectionstring))
            {
                var query = @"
            SELECT 
                mi.menu_id,
                mi.menu_name,
                mi.menu_img,
                cm.cuisine_name
            FROM vendores.tbl_restaurant r
            JOIN vendores.tbl_menu_items mi ON r.restaurant_id = mi.Restaurant_id
            JOIN vendores.tbl_cuisine vc ON r.restaurant_id = vc.Restaurnat_id
            JOIN admins.tbl_cuisine_master cm ON vc.cuisine_id = cm.cuisine_id
            WHERE r.restaurant_name = @RestaurantName
            AND mi.menu_img IS NOT NULL";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RestaurantName", restaurantName);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menuImages.Add(new MenuItemViewModel
                            {
                                MenuId = reader.GetInt32(0),
                                MenuName = reader.GetString(1),
                                MenuImg = reader["menu_img"] as byte[],
                                cuisine_name = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return menuImages;

        }

        public List<tbl_cuisine_master> GetAllCuisines()
        {
            List<tbl_cuisine_master> cuisines = new List<tbl_cuisine_master>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "SELECT cuisine_id, cuisine_name FROM admins.tbl_cuisine_master";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cuisines.Add(new tbl_cuisine_master
                    {
                        cuisine_id = (int)reader["cuisine_id"],
                        cuisine_name = reader["cuisine_name"].ToString()
                    });
                }
            }

            return cuisines;
        }

        public List<MenuItemViewModel> GetMenuItems(string restaurantName)
        {
            var list = new List<MenuItemViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT menu_id, menu_name, cuisine_id, menu_img FROM vendores.tbl_menu_items vmi INNER JOIN vendores.tbl_restaurant rs ON vmi.Restaurant_id = rs.restaurant_id WHERE rs.restaurant_name = @restaurantName";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@restaurantName", restaurantName);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new MenuItemViewModel
                    {
                        MenuId = Convert.ToInt32(rdr["menu_id"]),
                        MenuName = rdr["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(rdr["cuisine_id"]),
                        MenuImageBase64 = Convert.ToBase64String((byte[])rdr["menu_image"])
                    });
                }
            }
            return list;
        }

        public List<MenuItemViewModel> GetMenuItemsByCuisine(string restaurantName, int cuisineId)
        {
            var list = new List<MenuItemViewModel>();
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT menu_id, menu_name, cuisine_id, menu_img FROM vendores.tbl_menu_items  vmi INNER JOIN vendores.tbl_restaurant rs ON vmi.Restaurant_id = rs.restaurant_id WHERE restaurant_name = @restaurantName AND cuisine_id = @cuisineId ";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@restaurantName", restaurantName);
                cmd.Parameters.AddWithValue("@cuisineId", cuisineId);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new MenuItemViewModel
                    {
                        MenuId = Convert.ToInt32(rdr["menu_id"]),
                        MenuName = rdr["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(rdr["cuisine_id"]),
                        MenuImageBase64 = Convert.ToBase64String((byte[])rdr["menu_image"])
                    });
                }
            }
            return list;
        }

        // Repository - Updated with better debugging
        public List<CustomerRatingViewModel> GetRatingByRestaurant(string restaurantName)
        {
            var list = new List<CustomerRatingViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                {
                    string query = @"SELECT r.rating_id, r.customer_id, r.restaurant_id, r.order_id, r.rating,r.discription, r.created_at, c.customer_name
                            FROM customers.tbl_rating r
                            INNER JOIN customers.tbl_customer c ON r.customer_id = c.customer_id
                            INNER JOIN vendores.tbl_restaurant rest ON r.restaurant_id = rest.restaurant_id
                            WHERE rest.restaurant_name = @restaurantName
                            ORDER BY r.created_at DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@restaurantName", restaurantName);

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var review = new CustomerRatingViewModel
                            {
                                RatingId = rdr["rating_id"] != DBNull.Value ? Convert.ToInt32(rdr["rating_id"]) : 0,
                                CustomerId = rdr["customer_id"] != DBNull.Value ? Convert.ToInt32(rdr["customer_id"]) : 0,
                                RestaurantId = rdr["restaurant_id"] != DBNull.Value ? Convert.ToInt32(rdr["restaurant_id"]) : 0,
                                OrderId = rdr["order_id"] != DBNull.Value ? Convert.ToInt32(rdr["order_id"]) : 0,
                                Rating = rdr["rating"] != DBNull.Value ? Convert.ToInt32(rdr["rating"]) : 0,
                                Description = rdr["discription"] != DBNull.Value ? rdr["discription"].ToString() : string.Empty,
                                CreatedAt = rdr["created_at"] != DBNull.Value ? Convert.ToDateTime(rdr["created_at"]) : DateTime.MinValue,
                                CustomerName = rdr["customer_name"] != DBNull.Value ? rdr["customer_name"].ToString() : "Anonymous"
                            };

                            // Debug output
                            Console.WriteLine($"Found review: {review.RatingId} by {review.CustomerName}");

                            list.Add(review);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRatingByRestaurant: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            return list;
        }

        public bool AddRating(CustomerRatingViewModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                {
                    string query = @"INSERT INTO customers.tbl_rating 
                               (customer_id, restaurant_id, order_id, rating, description, created_at)
                               VALUES (@customerId, @restaurantId, @orderId, @rating, @description, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@customerId", model.CustomerId);
                    cmd.Parameters.AddWithValue("@restaurantId", model.RestaurantId);
                    cmd.Parameters.AddWithValue("@orderId", model.OrderId);
                    cmd.Parameters.AddWithValue("@rating", model.Rating);
                    cmd.Parameters.AddWithValue("@description", model.Description ?? string.Empty);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddRating: {ex.Message}");
                return false;
            }
        }

        public bool UpdateRating(CustomerRatingViewModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                {
                    string query = @"UPDATE customers.tbl_rating 
                               SET rating = @rating, 
                                   description = @description,
                                   created_at = GETDATE()
                               WHERE rating_id = @ratingId AND customer_id = @customerId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ratingId", model.RatingId);
                    cmd.Parameters.AddWithValue("@customerId", model.CustomerId);
                    cmd.Parameters.AddWithValue("@rating", model.Rating);
                    cmd.Parameters.AddWithValue("@description", model.Description ?? string.Empty);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateRating: {ex.Message}");
                return false;
            }
        }

        public CustomerRatingViewModel GetRatingById(int ratingId, int customerId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionstring))
                {
                    string query = @"SELECT rating_id, customer_id, restaurant_id, order_id, 
                               rating, description, created_at
                               FROM customers.tbl_rating
                               WHERE rating_id = @ratingId AND customer_id = @customerId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ratingId", ratingId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new CustomerRatingViewModel
                            {
                                RatingId = rdr["rating_id"] != DBNull.Value ? Convert.ToInt32(rdr["rating_id"]) : 0,
                                CustomerId = rdr["customer_id"] != DBNull.Value ? Convert.ToInt32(rdr["customer_id"]) : 0,
                                RestaurantId = rdr["restaurant_id"] != DBNull.Value ? Convert.ToInt32(rdr["restaurant_id"]) : 0,
                                OrderId = rdr["order_id"] != DBNull.Value ? Convert.ToInt32(rdr["order_id"]) : 0,
                                Rating = rdr["rating"] != DBNull.Value ? Convert.ToInt32(rdr["rating"]) : 0,
                                Description = rdr["description"] != DBNull.Value ? rdr["description"].ToString() : string.Empty,
                                CreatedAt = rdr["created_at"] != DBNull.Value ? Convert.ToDateTime(rdr["created_at"]) : DateTime.MinValue
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRatingById: {ex.Message}");
            }

            return null;
        }


        public void ClearCartAfterOrder(int customer_id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT cart_id FROM customers.tbl_cart WHERE customer_id = @CustomerId", conn);
                cmd.Parameters.AddWithValue("@CustomerId", customer_id);
                object result = cmd.ExecuteScalar();

                if(result != null)
                {
                    int cartId = Convert.ToInt32(result);

                    SqlCommand deleteItemsCmd = new SqlCommand("DELETE FROM customers.tbl_cart_item WHERE cart_id = @CartId", conn);
                    deleteItemsCmd.Parameters.AddWithValue("@CartId", cartId);
                    deleteItemsCmd.ExecuteNonQuery();

                    SqlCommand deleteCartCmd = new SqlCommand("DELETE FROM customers.tbl_cart WHERE customer_id = @CustomerId", conn);
                    deleteCartCmd.Parameters.AddWithValue("@CustomerId", customer_id);
                    deleteCartCmd.ExecuteNonQuery();
                }
            }
        }

        public List<MenuItemViewModel> GetDeliveryItemsByOrderId(int orderId)
        {
            var menuItems = new List<MenuItemViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"  SELECT mi.menu_id, mi.menu_name, mi.menu_img, mi.amount, mi.menu_descripation, r.restaurant_name
        FROM vendores.tbl_menu_items mi
        INNER JOIN customers.tbl_order_items oi ON mi.menu_id = oi.menu_id
        INNER JOIN vendores.tbl_restaurant r ON mi.restaurant_id = r.restaurant_id
        WHERE oi.order_id = @OrderId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string base64Image = string.Empty;

                    if (reader["menu_img"] != DBNull.Value)
                    {
                        byte[] imgBytes = (byte[])reader["menu_img"];
                        base64Image = Convert.ToBase64String(imgBytes);
                    }

                    menuItems.Add(new MenuItemViewModel
                    {
                        MenuId = Convert.ToInt32(reader["menu_id"]),
                        MenuName = reader["menu_name"].ToString(),
                        MenuImageBase64 = base64Image,
                        Amount = Convert.ToDecimal(reader["amount"]),
                        MenuDescription = reader["menu_descripation"].ToString(),
                        RestaurantName = reader["restaurant_name"].ToString()
                    });
                }
                conn.Close();
            }
            return menuItems;
        }

        public List<RestaurantViewModel> GetRecommendedRestaurants(int count = 6)
        {
            var restaurants = new List<RestaurantViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT TOP (@count) 
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street,
                            r.restaurant_pincode,
                            AVG(cr.rating) AS average_rating,
                            vi.Restaurant_menu_img,
                            STRING_AGG(cm.cuisine_name, ', ') AS cuisines
                        FROM vendores.tbl_restaurant r
                        INNER JOIN vendores.tbl_vendores_img vi ON r.restaurant_id = vi.Restaurant_id
                        INNER JOIN vendores.tbl_cuisine tc ON r.restaurant_id = tc.Restaurnat_id
                        INNER JOIN admins.tbl_cuisine_master cm ON tc.cuisine_id = cm.cuisine_id
                        INNER JOIN customers.tbl_rating cr ON cr.restaurant_id = r.restaurant_id
                        WHERE r.restaurant_isApprov = 1
                        GROUP BY 
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street,
                            r.restaurant_pincode,
                            vi.Restaurant_menu_img
                        ORDER BY average_rating DESC";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@count", count);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read()) { 
                    restaurants.Add(new RestaurantViewModel
                    {
                        RestaurantId = Convert.ToInt32(rd["restaurant_id"]),
                        RestaurantName = rd["restaurant_name"].ToString(),
                        Address = $"{rd["restaurant_street"]} {rd["restaurant_pincode"]}",
                        Rating = rd["average_rating"] != DBNull.Value ? Convert.ToDecimal(rd["average_rating"]) : 0,
                        RestaurantImageBase64 = rd["Restaurant_menu_img"] != DBNull.Value ? Convert.ToBase64String((byte[])rd["Restaurant_menu_img"]) : string.Empty,
                        CuisineName = rd["cuisines"] != DBNull.Value ? rd["cuisines"].ToString() : string.Empty
                    });
                }
                conn.Close();
            }
            return restaurants;
        }

        public List<RestaurantViewModel> GetRestaurantsByCuisine(int cuisineId)
        {
            var restaurants = new List<RestaurantViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT 
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street, 
                            r.restaurant_pincode, 
                            AVG(cr.rating) AS average_rating,
                            vi.Restaurant_menu_img,
                            cm.cuisine_name
                        FROM vendores.tbl_restaurant r
                        INNER JOIN vendores.tbl_vendores_img vi ON r.restaurant_id = vi.Restaurant_id
                        INNER JOIN vendores.tbl_cuisine tc ON r.restaurant_id = tc.Restaurnat_id
                        INNER JOIN admins.tbl_cuisine_master cm ON tc.cuisine_id = cm.cuisine_id
                        INNER JOIN customers.tbl_rating cr ON cr.restaurant_id = r.restaurant_id
                        WHERE r.restaurant_isApprov = 1 AND cm.cuisine_id = @cuisineId
                        GROUP BY
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street, 
                            r.restaurant_pincode,
                            vi.Restaurant_menu_img,
                            cm.cuisine_name
                        ORDER BY average_rating DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cuisineId", cuisineId);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    restaurants.Add(new RestaurantViewModel
                    {
                        RestaurantId = Convert.ToInt32(rd["restaurant_id"]),
                        RestaurantName = rd["restaurant_name"].ToString(),
                        Address = $"{rd["restaurant_street"]} {rd["restaurant_pincode"]}",
                        Rating = rd["average_rating"] != DBNull.Value ? Convert.ToDecimal(rd["average_rating"]) : 0,
                        RestaurantImageBase64 = rd["Restaurant_menu_img"] != DBNull.Value ? Convert.ToBase64String((byte[])rd["Restaurant_menu_img"]) : string.Empty,
                        CuisineName = rd["cuisine_name"] != DBNull.Value ? rd["cuisine_name"].ToString() : string.Empty
                    });
                }
                conn.Close();
            }
            return restaurants;
        }

        public List<RestaurantViewModel> SearchRestaurants(string searchTerm)
        {
            var restaurants = new List<RestaurantViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT 
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street, 
                            r.restaurant_pincode, 
                            AVG(cr.rating) AS average_rating,
                            vi.Restaurant_menu_img,
                            STRING_AGG(cm.cuisine_name, ', ') AS cuisines
                        FROM vendores.tbl_restaurant r
                        INNER JOIN vendores.tbl_vendores_img vi ON r.restaurant_id = vi.Restaurant_id
                        INNER JOIN vendores.tbl_cuisine tc ON r.restaurant_id = tc.Restaurnat_id
                        INNER JOIN admins.tbl_cuisine_master cm ON tc.cuisine_id = cm.cuisine_id
                        INNER JOIN customers.tbl_rating cr ON cr.restaurant_id = r.restaurant_id
                        WHERE r.restaurant_isApprov = 1 AND (r.restaurant_name LIKE @searchTerm OR cm.cuisine_name LIKE @searchTerm)
                        GROUP BY 
                            r.restaurant_id, 
                            r.restaurant_name, 
                            r.restaurant_street, 
                            r.restaurant_pincode,
                            vi.Restaurant_menu_img
                        ORDER BY average_rating DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    restaurants.Add(new RestaurantViewModel
                    {
                        RestaurantId = Convert.ToInt32(rd["restaurant_id"]),
                        RestaurantName = rd["restaurant_name"].ToString(),
                        Address = $"{rd["restaurant_street"]} {rd["restaurant_pincode"]}",
                        Rating = rd["average_rating"] != DBNull.Value ? Convert.ToDecimal(rd["average_rating"]) : 0,
                        RestaurantImageBase64 = rd["Restaurant_menu_img"] != DBNull.Value ? Convert.ToBase64String((byte[])rd["Restaurant_menu_img"]) : string.Empty,
                        CuisineName = rd["cuisines"] != DBNull.Value ? rd["cuisines"].ToString() : string.Empty
                    });
                }
                conn.Close();
            }
            return restaurants;
        }

        public bool ClearCustomerCart(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete all cart items for this customer
                        string sql = @"
                DELETE ci
                FROM customers.tbl_cart_item ci
                INNER JOIN customers.tbl_cart c ON ci.cart_id = c.cart_id
                WHERE c.customer_id = @CustomerId";

                        SqlCommand cmd = new SqlCommand(sql, conn, transaction);
                        cmd.Parameters.AddWithValue("@CustomerId", customerId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        transaction.Commit();

                        return rowsAffected >= 0; // Returns true even if cart was empty
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log error (ex)
                        throw; // Or return false if you prefer
                    }
                }
            }
        }

        public List<tbl_orders> GetUserOrdersWithItemsAndImages(int userId)
        {
            var orders = new List<tbl_orders>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                // Get orders
                string orderQuery = @"
                    SELECT co.*,cc.*,ca.address+' , '+ca.landmark+' | Romm No '+ca.door_no as address FROM customers.tbl_orders co
inner join customers.tbl_customer cc on co.customer_id = cc.customer_id
inner join customers.tbl_address ca on ca.customer_id = co.customer_id
WHERE cc.customer_id = @CustomerId
ORDER BY order_date DESC";

                SqlCommand orderCmd = new SqlCommand(orderQuery, conn);
                orderCmd.Parameters.AddWithValue("@CustomerId", userId);

                using (var reader = orderCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = new tbl_orders
                        {
                            order_id = Convert.ToInt32(reader["order_id"]),
                            customer_id = Convert.ToInt32(reader["customer_id"]),
                            order_date = Convert.ToDateTime(reader["order_date"]),
                            order_status = reader["order_status"].ToString(),
                            grand_total = Convert.ToDecimal(reader["grand_total"]),
                            razorpay_payment_id = reader["razorpay_payment_id"].ToString(),
                            razorpay_order_id = reader["razorpay_order_id"].ToString(),
                            razorpay_signature = reader["razorpay_signature"].ToString(),
                            address_id = Convert.ToInt32(reader["addressid"]),
                            customer_name = reader["customer_name"].ToString(), // adjust column name
                            delivery_address = reader["address"].ToString()
                        };
                        orders.Add(order);
                    }
                }

                // Get order items for each order
                foreach (var order in orders)
                {
                    string itemQuery = @"
                        SELECT oi.order_items_id, oi.order_id, oi.menu_id, oi.quantity, 
                           oi.list_price, oi.discount, oi.estimated_DATETIME,
                           mi.menu_name, mi.menu_img 
                            FROM customers.tbl_order_items oi
                            JOIN vendores.tbl_menu_items mi ON oi.menu_id = mi.menu_id
                            WHERE oi.order_id = @OrderId";

                    SqlCommand itemCmd = new SqlCommand(itemQuery, conn);
                    itemCmd.Parameters.AddWithValue("@OrderId", order.order_id);

                    using (var itemReader = itemCmd.ExecuteReader())
                    {
                        while (itemReader.Read())
                        {
                            order.OrderItems.Add(new tbl_order_items
                            {
                                order_item_id = Convert.ToInt32(itemReader["order_items_id"]),
                                order_id = Convert.ToInt32(itemReader["order_id"]),
                                menu_id = Convert.ToInt32(itemReader["menu_id"]),
                                quantity = Convert.ToInt32(itemReader["quantity"]),
                                list_price = Convert.ToDecimal(itemReader["list_price"]),
                                discount = Convert.ToDecimal(itemReader["discount"]),
                                estimated_time = Convert.ToDateTime(itemReader["estimated_DATETIME"]),
                                menu_name = itemReader["menu_name"].ToString(),
                                menu_img = itemReader["menu_img"] as byte[]
                            });
                        }
                    }
                }
            }

            return orders;
        }

        public bool SubmitReview(tbl_ratings ratings)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                string query = @"
                    INSERT INTO customers.tbl_rating 
                (customer_id, restaurant_id, order_id, rating, discription, created_at)
                VALUES (@Customer_Id, @RestaurantId, @OrderId ,@Rating, @Discription, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Customer_Id", ratings.CustomerId);
                cmd.Parameters.AddWithValue("@RestaurantId", ratings.RestaurantId);
                cmd.Parameters.AddWithValue("@OrderId", ratings.OrderId);
                cmd.Parameters.AddWithValue("@Rating", ratings.RatingValue);
                cmd.Parameters.AddWithValue("@Discription", ratings.discription ?? string.Empty);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        //Generate Billing Based in Order
        public byte[] GenerateBill(int orderId)
        {
            var order = GetUserOrdersWithItemsAndImages(2) // Replace with dynamic customer ID
        .FirstOrDefault(o => o.order_id == orderId);

            if (order == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (PdfDocument document = new PdfDocument())
                {
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    XFont headerFont = new XFont("Arial", 18, XFontStyle.Bold);
                    XFont subHeaderFont = new XFont("Arial", 12, XFontStyle.Bold);
                    XFont normalFont = new XFont("Arial", 12, XFontStyle.Regular);

                    double y = 30;

                    // Restaurant Name
                    gfx.DrawString("Foodie Express", headerFont, XBrushes.Black, new XRect(0, y, page.Width, 0), XStringFormats.TopCenter);
                    y += 25;
                    gfx.DrawString("123 Main Street, Mumbai, India", normalFont, XBrushes.Black, new XRect(0, y, page.Width, 0), XStringFormats.TopCenter);
                    y += 15;
                    gfx.DrawString("Phone: +91 9876543210 | GSTIN: 27AABCU9603R1ZV", normalFont, XBrushes.Black, new XRect(0, y, page.Width, 0), XStringFormats.TopCenter);

                    // Line
                    y += 30;
                    gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
                    y += 10;

                    // Invoice Header
                    gfx.DrawString($"Invoice for Order #{order.order_id}", subHeaderFont, XBrushes.Black, 40, y);
                    y += 25;

                    // Customer & Order Info
                    gfx.DrawString($"Customer: {order.customer_name}", normalFont, XBrushes.Black, 40, y);
                    y += 20;
                    gfx.DrawString($"Order Date: {order.order_date:dd MMM yyyy}", normalFont, XBrushes.Black, 40, y);
                    y += 20;
                    gfx.DrawString($"Order Status: {order.order_status}", normalFont, XBrushes.Black, 40, y);
                    y += 20;
                    gfx.DrawString($"Delivery Address: {order.delivery_address}", normalFont, XBrushes.Black, 40, y);
                    y += 30;

                    // Table Header
                    gfx.DrawString("Item", subHeaderFont, XBrushes.Black, 40, y);
                    gfx.DrawString("Qty", subHeaderFont, XBrushes.Black, 280, y);
                    gfx.DrawString("Price", subHeaderFont, XBrushes.Black, 340, y);
                    gfx.DrawString("Total", subHeaderFont, XBrushes.Black, 420, y);
                    y += 10;
                    gfx.DrawLine(XPens.Gray, 40, y, page.Width - 40, y);
                    y += 15;

                    // Items Loop
                    foreach (var item in order.OrderItems)
                    {
                        string itemName = item.menu_name.Length > 40 ? item.menu_name.Substring(0, 37) + "..." : item.menu_name;
                        gfx.DrawString(itemName, normalFont, XBrushes.Black, 40, y);
                        gfx.DrawString(item.quantity.ToString(), normalFont, XBrushes.Black, 280, y);
                        gfx.DrawString("₹" + item.list_price.ToString("0.00"), normalFont, XBrushes.Black, 340, y);
                        gfx.DrawString("₹" + (item.quantity * item.list_price).ToString("0.00"), normalFont, XBrushes.Black, 420, y);
                        y += 20;

                        if (y > page.Height - 100)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = 30;
                        }
                    }

                    // Line
                    y += 10;
                    gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
                    y += 20;

                    // Total
                    gfx.DrawString("Grand Total:", subHeaderFont, XBrushes.Black, 340, y);
                    gfx.DrawString("₹" + order.grand_total.ToString("0.00"), subHeaderFont, XBrushes.Black, 420, y);

                    // Footer
                    y += 40;
                    gfx.DrawString("Thank you for ordering with Foodie Express!", normalFont, XBrushes.Black, new XRect(0, y, page.Width, 0), XStringFormats.TopCenter);

                    document.Save(ms);
                }

                return ms.ToArray();
            }
        }

        public List<tbl_orders> FilterOrders(int userId, string status, int? days)
        {
            var orders = GetUserOrdersWithItemsAndImages(userId);

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.order_status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if(days.HasValue)
            {
                DateTime cutoffDate = DateTime.Now.AddDays(-days.Value);
                orders = orders.Where(o => o.order_date >= cutoffDate).ToList();
            }
            return orders;
        }

        public IEnumerable<TopSellingMenuViewModel> topSellingMenuViewModels(int restaurantId, int count = 5)
        {
            var topMenus = new List<TopSellingMenuViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                var query = @"SELECT TOP (@Count)
                            mi.menu_id,
                            mi.menu_name,
                            mi.menu_descripation,
                            ISNULL(SUM(oi.quantity), 0) AS TotalQuantitySold,
                            ISNULL(SUM(oi.quantity * oi.list_price), 0) AS TotalRevenue,
                            mi.menu_img,
                            r.restaurant_name AS restaurant_name
                        FROM customers.tbl_order_items oi
                        INNER JOIN customers.tbl_orders o ON oi.order_id = o.order_id
                        INNER JOIN vendores.tbl_menu_items mi ON oi.menu_id = mi.menu_id
                        INNER JOIN vendores.tbl_restaurant r ON o.resturant_id = r.restaurant_id
                        WHERE o.resturant_id = @RestaurantId
                        GROUP BY mi.menu_id, mi.menu_name, mi.menu_img, r.restaurant_name, mi.menu_descripation
                        ORDER BY TotalQuantitySold DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);
                cmd.Parameters.AddWithValue("@Count", count);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topMenus.Add(new TopSellingMenuViewModel
                        {
                            MenuId = Convert.ToInt32(reader["menu_id"]),
                            MenuName = reader["menu_name"].ToString(),
                            MenuDescription = reader["menu_descripation"].ToString(),
                            TotalQuantitySold = Convert.ToInt32(reader["TotalQuantitySold"]),
                            TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                            ImageUrl = reader["menu_img"] != DBNull.Value ? Convert.ToBase64String((byte[])reader["menu_img"]) : string.Empty,
                            RestaurantName = reader["restaurant_name"].ToString(),
                            RestaurantId = restaurantId
                        });
                    }
                }
            }
            return topMenus;
        }
    }
}