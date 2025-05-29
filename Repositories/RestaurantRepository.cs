using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Microsoft.Data.SqlClient;
using System.Data;
using Foodie.ViewModels;
using Foodie.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;
using iTextFont = iTextSharp.text.Font;
using iTextFontFactory = iTextSharp.text.FontFactory;
using iTextBaseColor = iTextSharp.text.BaseColor;


namespace Foodie.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly string _connectionstring;
        private int o_id = 0;
        private static int r_id = 0;
        public static void setRId(int id)
        {
            r_id = id;
        }
        public static int getRId()
        {
            return r_id;
        }
        public RestaurantRepository(IConfiguration configuration,IWebHostEnvironment webHost)
        {
            _connectionstring = configuration.GetConnectionString("Defaultconnection");
        }

        public tbl_restaurant getRestaurant(int id)
        {
            tbl_restaurant tbl_Restaurant = null;
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_restaurant where restaurant_id = @id";

                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
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
            using (SqlConnection conn = new SqlConnection(_connectionstring))
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
                string query = @"INSERT INTO vendores.tbl_restaurant (restaurant_name, restaurant_contact, restaurant_email, restaurant_street, restaurant_pincode, restaurant_lat, restaurant_lag, restaurant_isApprov,restaurant_isOnline,est_id,owner_id,password) 
                                 VALUES (@restaurant_name, @restaurant_contact, @restaurant_email, @restaurant_street, @restaurant_pincode, @restaurant_lat, @restaurant_lag, @restaurant_isApprov,@restaurant_isOnline,1,@owner_id,@password);
                                    SELECT SCOPE_IDENTITY();";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@restaurant_name", restaurant.restaurant_name);
                cmd.Parameters.AddWithValue("@restaurant_contact", restaurant.restaurant_contact);
                cmd.Parameters.AddWithValue("@restaurant_email", restaurant.restaurant_email);
                cmd.Parameters.AddWithValue("@restaurant_street", restaurant.restaurant_street);
                cmd.Parameters.AddWithValue("@restaurant_pincode", restaurant.restaurant_pincode);
                cmd.Parameters.AddWithValue("@restaurant_lat", restaurant.restaurant_lat);
                cmd.Parameters.AddWithValue("@restaurant_lag", restaurant.restaurant_lag);
                cmd.Parameters.AddWithValue("@restaurant_isApprov", restaurant.restaurant_isApprov);
                cmd.Parameters.AddWithValue("@restaurant_isOnline", restaurant.restaurant_isOnline);
                cmd.Parameters.AddWithValue("@password", restaurant.rest_password);
                cmd.Parameters.AddWithValue("@owner_id", o_id);

                conn.Open();
                int result = Convert.ToInt32(cmd.ExecuteScalar());

                setRId(result);

                conn.Close();
                return result;
            }
        }

        public tbl_vendores_img getVendors_img(int id)
        {
            tbl_vendores_img tbl_Vendores_Img = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_vendores_img where restaurant_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    tbl_Vendores_Img = new tbl_vendores_img
                    {
                        Restaurant_img = (byte[])dr["Restaurant_img"],
                        Restaurant_menu_img = (byte[])dr["Restaurant_menu_img"]
                    };
                }
                conn.Close();
                return tbl_Vendores_Img;
            }
        }

        public tbl_vendor_availability getVendor_Available(int id)
        {
            tbl_vendor_availability tbl_Vendor_Availability = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_vendor_availability where restaurant_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    tbl_Vendor_Availability = new tbl_vendor_availability
                    {
                        open_time = Convert.ToDateTime(dr["open_datetime"]),
                        close_time = Convert.ToDateTime(dr["close_datetime"]),
                        day_of_week = dr["day_of_week"].ToString(),
                        is_Open = Convert.ToBoolean(dr["is_Open"])
                    };
                }
                conn.Close();
                return tbl_Vendor_Availability;
            }
        }

        public int AddVendors_img(byte[] r_img, byte[] m_img)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"insert into vendores.tbl_vendores_img(Restaurant_id,Restaurant_img,Restaurant_menu_img) values (@Restaurant_id,@Restaurant_img,@Restaurant_menu_img)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Restaurant_id", getRId());
                cmd.Parameters.AddWithValue("@Restaurant_img", r_img);
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

                string qry = "insert into vendores.tbl_vendor_availability(Restaurant_id,open_datetime,close_datetime,day_of_week,is_Open) values(@resid,@o_time,@c_time,@dyw,@open)";

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
                cmd.Parameters.AddWithValue("@rid", pan.Restaurant_id);
                cmd.Parameters.AddWithValue("@img", img);

                conn.Open();

                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return result;
            }
        }

        public int AddGSTDetails(tbl_gst_details gst)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "INSERT INTO vendores.tbl_gst_details (gst_number, is_Verify, Restaurant_id) VALUES (@number, @verify, @rid)";

                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    // Check if gst_number is null or empty
                    bool isGstNumberNull = string.IsNullOrWhiteSpace(gst.gst_number);

                    // Add parameters with appropriate null handling
                    cmd.Parameters.AddWithValue("@number", isGstNumberNull ? (object)DBNull.Value : gst.gst_number);
                    cmd.Parameters.AddWithValue("@verify", isGstNumberNull ? (object)DBNull.Value : gst.is_Verify);
                    cmd.Parameters.AddWithValue("@rid", isGstNumberNull ? (object)DBNull.Value : gst.Restaurant_id);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result;
                }
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

        public int AddMenu(tbl_menu_items menu)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "insert into vendores.tbl_menu_items(menu_name, cuisine_id, menu_img, menu_descripation, amount, isAvalable, Restaurant_id)values(@name, @cid, @img, @desc, @amt, @avai, @rid)";

                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@name", menu.menu_name);
                cmd.Parameters.AddWithValue("@cid", menu.cuisine_id);
                cmd.Parameters.AddWithValue("@img", menu.menu_img);
                cmd.Parameters.AddWithValue("@desc", menu.menu_descripation);
                cmd.Parameters.AddWithValue("@amt", menu.amount);
                cmd.Parameters.AddWithValue("@avai", menu.isAvailable);
                cmd.Parameters.AddWithValue("@rid", menu.Restaurant_id);

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;

            }
        }

        public List<tbl_menu_items> getMenuByRes(int id)
        {
            List<tbl_menu_items> tbl_Menu_Items = new List<tbl_menu_items>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_menu_items where restaurant_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tbl_Menu_Items.Add(new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(dr["menu_id"]),
                        menu_name = dr["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(dr["cuisine_id"]),
                        menu_img = (byte[])dr["menu_img"],
                        menu_descripation = dr["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(dr["amount"]),
                        isAvailable = Convert.ToBoolean(dr["isAvalable"]),
                        Restaurant_id = Convert.ToInt32(dr["Restaurant_id"])
                    });
                }
                conn.Close();
            }

            return tbl_Menu_Items;
        }

        public tbl_menu_items getMenu(int id)
        {
            tbl_menu_items tbl_Menu_Items = null;

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select * from vendores.tbl_menu_items where menu_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    tbl_Menu_Items = new tbl_menu_items
                    {
                        menu_id = Convert.ToInt32(dr["menu_id"]),
                        menu_name = dr["menu_name"].ToString(),
                        cuisine_id = Convert.ToInt32(dr["cuisine_id"]),
                        menu_img = (byte[])dr["menu_img"],
                        menu_descripation = dr["menu_descripation"].ToString(),
                        amount = Convert.ToDecimal(dr["amount"]),
                        isAvailable = Convert.ToBoolean(dr["isAvalable"]),
                        Restaurant_id = Convert.ToInt32(dr["Restaurant_id"])
                    };
                }
                conn.Close();
            }
            return tbl_Menu_Items;
        }

        public int DeleteMenu(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "delete from vendores.tbl_menu_items where menu_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public int UpdateMenu(tbl_menu_items menu)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "update vendores.tbl_menu_items set menu_name = @name, cuisine_id = @cid, menu_img = @img, menu_descripation = @desc, amount = @amt, isAvalable = @avai where menu_id = @id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@name", menu.menu_name);
                cmd.Parameters.AddWithValue("@cid", menu.cuisine_id);
                cmd.Parameters.AddWithValue("@img", menu.menu_img);
                cmd.Parameters.AddWithValue("@desc", menu.menu_descripation);
                cmd.Parameters.AddWithValue("@amt", menu.amount);
                cmd.Parameters.AddWithValue("@avai", menu.isAvailable);
                cmd.Parameters.AddWithValue("@id", menu.menu_id);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public List<tbl_orders_notifi> tbl_Orders_Notifis(int restaurant_id)
        {
            List<tbl_orders_notifi> orders = new List<tbl_orders_notifi>();


            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "select top 1 coi.*,co.food_status,co.resturant_id from customers.tbl_orders co inner join customers.tbl_order_items coi on co.order_id = coi.order_id where food_status = 'waiting' and co.resturant_id = @RestaurantId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RestaurantId", restaurant_id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tbl_orders_notifi order = new tbl_orders_notifi
                    {
                        order_items_id = Convert.ToInt32(reader["order_items_id"]),
                        order_id = Convert.ToInt32(reader["order_id"]),
                        restaurant_id = Convert.ToInt32(reader["resturant_id"]),
                        menu_id = Convert.ToInt32(reader["menu_id"]),
                        quantity = Convert.ToInt32(reader["quantity"]),
                        list_price = Convert.ToInt32(reader["list_price"]),
                        discount = Convert.ToDecimal(reader["discount"]),
                        estimated_time = Convert.ToDateTime(reader["estimated_DATETIME"]),
                        food_status = reader["food_status"].ToString()
                    };

                    orders.Add(order);
                }

                conn.Close();
            }

            return orders;
        }

        public int AcceptOrder(int order_id, string food_status)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "update customers.tbl_orders set food_status = @food_status where order_id = @order_id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@order_id", order_id);
                cmd.Parameters.AddWithValue("@food_status", food_status);

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public List<ordersViewMdel> tbl_Orders_Notifis_Accepted(int restaurant_id)
        {
            var orders = new List<ordersViewMdel>();


            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                string orderQuery = @"SELECT DISTINCT co.order_id, co.food_status, cc.customer_name
                              FROM customers.tbl_orders co
                              INNER JOIN customers.tbl_customer cc ON co.customer_id = cc.customer_id
                              INNER JOIN vendores.tbl_restaurant vs ON co.resturant_id = vs.restaurant_id
                              WHERE co.food_status in ('ACCEPT','completed','PickUp')
                                AND co.resturant_id = @RestaurantId 
                                AND vs.restaurant_isOnline = 1";

                using (SqlCommand orderCmd = new SqlCommand(orderQuery, conn))
                {
                    orderCmd.Parameters.AddWithValue("@RestaurantId", restaurant_id);

                    using (SqlDataReader reader = orderCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new ordersViewMdel
                            {
                                order_id = Convert.ToInt32(reader["order_id"]),
                                food_status = reader["food_status"].ToString(),
                                customer_name = reader["customer_name"].ToString(),
                                Dishes = new List<orderNotiViewModel>()
                            });
                        }
                    }
                }
                conn.Close(); // close connection after reading orders
            }

            // Second: Load all items for each order
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                foreach (var order in orders)
                {
                    string itemQuery = @"SELECT quantity,estimated_DATETIME, list_price, discount ,vm.menu_name,sum(list_price * quantity * (1 - discount))['Total Price']
                                        FROM customers.tbl_order_items coi
                                        inner join vendores.tbl_menu_items vm
                                        on coi.menu_id = vm.menu_id WHERE order_id = @OrderId
										group by quantity,estimated_DATETIME, list_price, discount ,vm.menu_name";

                    using (SqlCommand itemCmd = new SqlCommand(itemQuery, conn))
                    {
                        itemCmd.Parameters.AddWithValue("@OrderId", order.order_id);

                        using (SqlDataReader reader = itemCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                order.Dishes.Add(new orderNotiViewModel
                                {
                                    menu_name = reader["menu_name"].ToString(),
                                    quantity = Convert.ToInt32(reader["quantity"]),
                                    list_price = Convert.ToInt32(reader["list_price"]),
                                    discount = Convert.ToDecimal(reader["discount"]),
                                    estimated_time = Convert.ToDateTime(reader["estimated_DATETIME"]),
                                });
                            }
                        }
                    }
                }
                conn.Close();
            }

            return orders;
        }

        public int IsOnline(int restaurant_id, int isOnline)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "update vendores.tbl_restaurant set restaurant_isOnline = @isOnline where restaurant_id = @restaurant_id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@isOnline", isOnline);
                cmd.Parameters.AddWithValue("@restaurant_id", restaurant_id);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public int getOnline(int restaurant_id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select restaurant_isOnline from vendores.tbl_restaurant where restaurant_id = @resid";
                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@resid", restaurant_id);

                conn.Open();
                int result = Convert.ToInt32(cmd.ExecuteScalar());


                return result;



            }
        }
        public bool isApprove(int restaurant_id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "select restaurant_isApprov from vendores.tbl_restaurant where  restaurant_id = @restaurant_id";
                SqlCommand cmd = new SqlCommand(qry, conn);

                cmd.Parameters.AddWithValue("@restaurant_id", restaurant_id);
                conn.Open();
                bool result = (bool)cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
        }

        public int OrderReady(int order_id, int restaurant_id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string qry = "update customers.tbl_orders set food_status = 'completed' where order_id = @order_id and resturant_id = @restaurant_id";
                SqlCommand cmd = new SqlCommand(qry, conn);
                cmd.Parameters.AddWithValue("@order_id", order_id);
                cmd.Parameters.AddWithValue("@restaurant_id", restaurant_id);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                conn.Close();
                return result;
            }
        }

        public List<ordersViewMdel> tbl_Orders_History(int restaurant_id)
        {
            var orders = new List<ordersViewMdel>();


            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                string orderQuery = @"SELECT  co.order_id, co.food_status, cc.customer_name
                                    FROM customers.tbl_orders co
                                    INNER JOIN customers.tbl_customer cc ON co.customer_id = cc.customer_id
                                    INNER JOIN vendores.tbl_restaurant vs ON co.resturant_id = vs.restaurant_id
                                    AND co.resturant_id =@RestaurantId";

                using (SqlCommand orderCmd = new SqlCommand(orderQuery, conn))
                {
                    orderCmd.Parameters.AddWithValue("@RestaurantId", restaurant_id);

                    using (SqlDataReader reader = orderCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new ordersViewMdel
                            {
                                order_id = Convert.ToInt32(reader["order_id"]),
                                food_status = reader["food_status"].ToString(),
                                customer_name = reader["customer_name"].ToString(),
                                Dishes = new List<orderNotiViewModel>()
                            });
                        }
                    }
                }
                conn.Close(); // close connection after reading orders
            }

            // Second: Load all items for each order
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                foreach (var order in orders)
                {
                    string itemQuery = @"SELECT quantity,estimated_DATETIME, list_price, discount ,vm.menu_name
                                            FROM customers.tbl_order_items coi
                                            inner join vendores.tbl_menu_items vm
                                            on coi.menu_id = vm.menu_id WHERE order_id = @OrderId";

                    using (SqlCommand itemCmd = new SqlCommand(itemQuery, conn))
                    {
                        itemCmd.Parameters.AddWithValue("@OrderId", order.order_id);

                        using (SqlDataReader reader = itemCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                order.Dishes.Add(new orderNotiViewModel
                                {
                                    menu_name = reader["menu_name"].ToString(),
                                    quantity = Convert.ToInt32(reader["quantity"]),
                                    list_price = Convert.ToInt32(reader["list_price"]),
                                    discount = Convert.ToDecimal(reader["discount"]),
                                    estimated_time = Convert.ToDateTime(reader["estimated_DATETIME"]),
                                });
                            }
                        }
                    }
                }
                conn.Close();
            }

            return orders;
        }

        public OutletInfo GetOutletInfo(int id)
        {
            using var con = new SqlConnection(_connectionstring);
            using var cmd = new SqlCommand("vendores.usp_GetOutletInfo", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RestaurantId", id);

            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new OutletInfo
                {
                    restaurant_id = (int)reader["restaurant_id"],
                    restaurant_name = reader["restaurant_name"].ToString(),
                    restaurant_street = reader["restaurant_street"].ToString(),
                    restaurant_pincode = reader["restaurant_pincode"].ToString(),
                    restaurant_phone = reader["restaurant_contact"].ToString(),
                    restaurant_email = reader["restaurant_email"].ToString(),
                    restaurant_isOnline = (bool)reader["restaurant_isOnline"],
                    restaurant_img = reader["Restaurant_img"] as byte[],
                    restaurant_menu_img = reader["Restaurant_menu_img"] as byte[]
                };
            }
            return null;
        }

        public void UpdateOutletInfo(OutletInfo model)
        {
            using var con = new SqlConnection(_connectionstring);
            using var cmd = new SqlCommand("vendores.usp_UpdateOutletInfo", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@restaurant_id", model.restaurant_id);
            cmd.Parameters.AddWithValue("@restaurant_name", model.restaurant_name);
            cmd.Parameters.AddWithValue("@restaurant_street", model.restaurant_street);
            cmd.Parameters.AddWithValue("@restaurant_pincode", model.restaurant_pincode);
            cmd.Parameters.AddWithValue("@restaurant_contact", model.restaurant_phone);
            cmd.Parameters.AddWithValue("@restaurant_email", model.restaurant_email);
            cmd.Parameters.AddWithValue("@restaurant_isOnline", model.restaurant_isOnline);

            cmd.Parameters.Add("@Restaurant_img", SqlDbType.VarBinary).Value = (object?)model.restaurant_img ?? DBNull.Value;
            cmd.Parameters.Add("@Restaurant_menu_img", SqlDbType.VarBinary).Value = (object?)model.restaurant_menu_img ?? DBNull.Value;

            con.Open();
            cmd.ExecuteNonQuery();
        }



        public List<tbl_ratings> GetAllRatings(int restaurant_id)
            {
            var ratings = new List<tbl_ratings>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("customers.sp_get_all_ratings", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@restaurant_id", restaurant_id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ratings.Add(new tbl_ratings
                        {
                            RatingId = (int)reader["rating_id"],
                            customer_name = reader["customer_name"]?.ToString(),
                            restaurant_name = reader["restaurant_name"]?.ToString(),
                            OrderId = reader["order_id"] as int?,
                            craetedAt = Convert.ToDateTime(reader["created_at"]),
                            RatingValue = (int)reader["rating"],
                            discription = reader["discription"]?.ToString(),
                            image = reader["profilepic"] != DBNull.Value ? (byte[])reader["profilepic"] : null
                        });
                    }
                }
            }

            return ratings;
        }


        public IEnumerable<tbl_cust_vendor_complaints> GetComplaintsByRestaurantId(int restaurantId)
        {
            var complaints = new List<tbl_cust_vendor_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_get_complaints_by_restaurant_id", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@restaurant_id", restaurantId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    complaints.Add(new tbl_cust_vendor_complaints
                    {
                        vendor_complaint_id = reader["vendor_complaint_id"] != DBNull.Value ? Convert.ToInt32(reader["vendor_complaint_id"]) : 0,
                        restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        customer_name = reader["customer_name"] != DBNull.Value ? reader["customer_name"].ToString() : string.Empty,
                        cmp_Descr = reader["cmp_Descr"] != DBNull.Value ? reader["cmp_Descr"].ToString() : string.Empty,
                        cmp_Status = reader["cmp_Status"] != DBNull.Value && Convert.ToBoolean(reader["cmp_Status"]),
                        ResolutionRemarks = reader["ResolutionRemarks"] != DBNull.Value ? reader["ResolutionRemarks"].ToString() : null,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.Now,
                        ResolvedAt = reader["ResolvedAt"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(reader["ResolvedAt"])
                    });

                }
            }

            return complaints;
        }

        public void updateVencom(tbl_cust_vendor_complaints tbl_Cust_Vendor_Complaints)
        {
            using (var conn = new SqlConnection(_connectionstring))
            {
                //conn.Open();

                using (var command = new SqlCommand("admins.sp_edit_cust_complaints", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@vendor_com_id", tbl_Cust_Vendor_Complaints.vendor_complaint_id);
                    command.Parameters.AddWithValue("@cmp_status", 1);
                    command.Parameters.AddWithValue("@restaurant_id", tbl_Cust_Vendor_Complaints.restaurant_id);
                    command.Parameters.AddWithValue("@resolutionRemarks", tbl_Cust_Vendor_Complaints.ResolutionRemarks);
                    command.Parameters.AddWithValue("@resolvedAt", DateTime.Now);
                    // Execute the stored procedure
                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public IEnumerable<tbl_special_offers> GetAllOffers()
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_special_offers";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    offers.Add(new tbl_special_offers()
                    {
                        so_id = Convert.ToInt32(rd["so_id"]),
                        restaurant_id = Convert.ToInt32(rd["restaurant_id"]),
                        offer_title = rd["offer_title"].ToString(),
                        offer_desc = rd["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(rd["percentage_disc"]),
                        validFrom = Convert.ToDateTime(rd["validFrom"]),
                        validTo = Convert.ToDateTime(rd["validTo"]),
                        is_Active = Convert.ToBoolean(rd["is_Active"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        ImagePath = rd["image_path"].ToString()
                    });
                }
                conn.Close();
            }
            return offers;
        }

        public void AddOffeer(tbl_special_offers offers)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"INSERT INTO vendores.tbl_special_offers(restaurant_id,offer_title,offer_desc,percentage_disc,validFrom,validTo,is_Active,menu_id,image_path) VALUES (@restaurant_id, @offer_title, @offer_desc, @percentage_disc, @validFrom, @validTo, @is_Active, @menu_id, @ImagePath)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@restaurant_id", offers.restaurant_id);
                cmd.Parameters.AddWithValue("@offer_title", offers.offer_title);
                cmd.Parameters.AddWithValue("@offer_desc", offers.offer_desc ?? "");
                cmd.Parameters.AddWithValue("@percentage_disc", offers.percentage_disc);
                cmd.Parameters.AddWithValue("@validFrom", offers.validFrom);
                cmd.Parameters.AddWithValue("@validTo", offers.validTo);
                cmd.Parameters.AddWithValue("@is_Active", offers.is_Active);
                cmd.Parameters.AddWithValue("@menu_id", offers.menu_id);
                cmd.Parameters.AddWithValue("@ImagePath", offers.ImagePath ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public tbl_special_offers GetOfferById(int id)
        {
            tbl_special_offers offer = null;

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_special_offers where so_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if(reader.Read())
                {
                    offer = new tbl_special_offers
                    {
                        so_id = Convert.ToInt32(reader["so_id"]),
                        restaurant_id = Convert.ToInt32(reader["restaurant_id"]),
                        offer_title = reader["offer_title"].ToString(),
                        offer_desc = reader["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(reader["percentage_disc"]),
                        validFrom = Convert.ToDateTime(reader["validFrom"]),
                        validTo = Convert.ToDateTime(reader["validTo"]),
                        is_Active = Convert.ToBoolean(reader["is_Active"]),
                        menu_id = Convert.ToInt32(reader["menu_id"]),
                        ImagePath = reader["image_path"].ToString()
                    };
                }
                conn.Close();
            }
            return offer;
        }

        public void UpdateOffer(tbl_special_offers offer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query;

                // If new image is uploaded, include ImagePath update
                if (!string.IsNullOrEmpty(offer.ImagePath))
                {
                    query = @"UPDATE vendores.tbl_special_offers SET 
                        restaurant_id = @restaurant_id,
                        offer_title = @offer_title,
                        offer_desc = @offer_desc,
                        percentage_disc = @percentage_disc,
                        validFrom = @validFrom,
                        validTo = @validTo,
                        is_Active = @is_Active,
                        menu_id = @menu_id,
                        ImagePath = @ImagePath
                      WHERE so_id = @so_id";
                }
                else
                {
                    query = @"UPDATE vendores.tbl_special_offers SET 
                        restaurant_id = @restaurant_id,
                        offer_title = @offer_title,
                        offer_desc = @offer_desc,
                        percentage_disc = @percentage_disc,
                        validFrom = @validFrom,
                        validTo = @validTo,
                        is_Active = @is_Active,
                        menu_id = @menu_id
                      WHERE so_id = @so_id";
                }

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@so_id", offer.so_id);
                cmd.Parameters.AddWithValue("@restaurant_id", offer.restaurant_id);
                cmd.Parameters.AddWithValue("@offer_title", offer.offer_title);
                cmd.Parameters.AddWithValue("@offer_desc", offer.offer_desc ?? "");
                cmd.Parameters.AddWithValue("@percentage_disc", offer.percentage_disc);
                cmd.Parameters.AddWithValue("@validFrom", offer.validFrom);
                cmd.Parameters.AddWithValue("@validTo", offer.validTo);
                cmd.Parameters.AddWithValue("@is_Active", offer.is_Active);
                cmd.Parameters.AddWithValue("@menu_id", offer.menu_id);

                if (!string.IsNullOrEmpty(offer.ImagePath))
                {
                    cmd.Parameters.AddWithValue("@ImagePath", offer.ImagePath);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void DeleteOffer(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = "DELETE FROM vendores.tbl_special_offers WHERE so_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        

        public void SaveOTP(string email, string otp)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                string query = "INSERT INTO tbl_email_otp (email, otp_code) VALUES (@Email, @OTP)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.ExecuteNonQuery();
            }
        }

        public bool ValidateOTP(string email, string otp)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                string query = @"SELECT created_at FROM tbl_email_otp 
                         WHERE email = @Email AND otp_code = @OTP AND is_verified = 0 
                         ORDER BY created_at DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    DateTime createdAt = Convert.ToDateTime(reader["created_at"]);
                    if ((DateTime.Now - createdAt).TotalSeconds <= 120)
                    {
                        reader.Close();

                        string update = "UPDATE tbl_email_otp SET is_verified = 1 WHERE email = @Email AND otp_code = @OTP";
                        SqlCommand updateCmd = new SqlCommand(update, conn);
                        updateCmd.Parameters.AddWithValue("@Email", email);
                        updateCmd.Parameters.AddWithValue("@OTP", otp);
                        updateCmd.ExecuteNonQuery();

                        return true;
                    }
                }
            }
            return false;
        }
        public PayoutsDetailsViewModel GetBankDetailsByRestaurantId(int restaurantId)
        {
            PayoutsDetailsViewModel details = new PayoutsDetailsViewModel();

            using (SqlConnection con = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("vendores.sp_GetBankDetailsByRestaurantId", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Restaurant_id", restaurantId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    details.BankDetails = new tbl_bank_details
                    {
                        bank_id = reader["bank_id"] != DBNull.Value ? Convert.ToInt32(reader["bank_id"]) : 0,
                        bank_name = reader["bank_name"] != DBNull.Value ? reader["bank_name"].ToString() : string.Empty,
                        IFSC_code = reader["IFSC_code"] != DBNull.Value ? reader["IFSC_code"].ToString() : string.Empty,
                        ACC_NO = reader["ACC_No"] != DBNull.Value ? reader["ACC_No"].ToString() : string.Empty,
                        ACC_Type = reader["ACC_Type"] != DBNull.Value ? reader["ACC_Type"].ToString() : string.Empty,
                        Restaurant_id = reader["Restaurant_id"] != DBNull.Value ? Convert.ToInt32(reader["Restaurant_id"]) : 0
                    };
                }
            }

            return details;
        }


        public List<OrderViewModel> GetWeeklyOrderStatsAsync(int restaurantid)
        {
            var result = new List<OrderViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand("customers.sp_GetWeeklyOrderStats", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@restaurantid", restaurantid);
                    conn.Open();
                    using (SqlDataReader reader =  cmd.ExecuteReader())
                    {
                        while ( reader.Read())
                        {
                            var stat = new OrderViewModel
                            {
                                WeekNumber = reader.GetInt32("WeekNumber"),
                                WeekLabel = reader.GetString("WeekLabel"),
                                BadOrders = reader.GetInt32("BadOrders"),
                                DelayedOrders = reader.GetInt32("DelayedOrders"),
                                CompletedOrders = reader.GetInt32("CompleteOrders")
                            };
                            result.Add(stat);
                        }
                    }
                }
            }

            return result;
        }

        public List<OrderViewModel> GetWeeklyCustomerRatings(int restaurantid)
        {
            var result = new List<OrderViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("sp_GetWeeklyCustomerRatings", conn))
            {

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@restaurantid", restaurantid);
                 conn.Open();
                using (var reader =  cmd.ExecuteReader())
                {
                    while ( reader.Read())
                    {
                        result.Add(new OrderViewModel
                        {
                            WeekNumber = reader.GetInt32(reader.GetOrdinal("WeekNumber")),
                            WeekLabel = reader.GetString(reader.GetOrdinal("WeekLabel")),
                            AverageRating = reader.GetDecimal(reader.GetOrdinal("AverageRating"))
                        });
                    }
                }
            }

            return result;
        }

        public List<reports> GetPerformanceMetrics()
        {
            var metrics = new List<reports>();

            using (var conn = new SqlConnection(_connectionstring))
            {
                using (var cmd = new SqlCommand("customers.sp_GetRestaurantPerformanceMetrics", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            metrics.Add(new reports
                            {
                                Metric = reader["Metric"].ToString(),
                                Value = Convert.ToDecimal(reader["Value"])
                            });
                        }
                    }
                }
            }

            return metrics;
        }

        public IEnumerable<tbl_special_offers> GetOffersByStatus(bool isActive)
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_special_offers WHERE is_Active = @isActive";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@isActive", isActive);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    offers.Add(new tbl_special_offers()
                    {
                        so_id = Convert.ToInt32(rd["so_id"]),
                        restaurant_id = Convert.ToInt32(rd["restaurant_id"]),
                        offer_title = rd["offer_title"].ToString(),
                        offer_desc = rd["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(rd["percentage_disc"]),
                        validFrom = Convert.ToDateTime(rd["validFrom"]),
                        validTo = Convert.ToDateTime(rd["validTo"]),
                        is_Active = Convert.ToBoolean(rd["is_Active"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        ImagePath = rd["image_path"].ToString()
                    });
                }
                conn.Close();
            }
            return offers;
        }

        public IEnumerable<tbl_special_offers> GetOffersByDateAndStatus(DateTime? validFrom, DateTime? validTo, bool? isActive)
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_special_offers 
                        WHERE (@validFrom IS NULL OR validFrom >= @validFrom) 
                        AND (@validTo IS NULL OR validTo <= @validTo)
                        AND (@isActive IS NULL OR is_Active = @isActive)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@validFrom", validFrom.HasValue ? (object)validFrom.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@validTo", validTo.HasValue ? (object)validTo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@isActive", isActive.HasValue ? (object)isActive.Value : DBNull.Value);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    offers.Add(new tbl_special_offers()
                    {
                        so_id = Convert.ToInt32(rd["so_id"]),
                        restaurant_id = Convert.ToInt32(rd["restaurant_id"]),
                        offer_title = rd["offer_title"].ToString(),
                        offer_desc = rd["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(rd["percentage_disc"]),
                        validFrom = Convert.ToDateTime(rd["validFrom"]),
                        validTo = Convert.ToDateTime(rd["validTo"]),
                        is_Active = Convert.ToBoolean(rd["is_Active"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        ImagePath = rd["image_path"].ToString()
                    });
                }
                conn.Close();
            }
            return offers;
        }

        public IEnumerable<tbl_special_offers> GetOffersByDateRange(DateTime? validFrom, DateTime? validTo)
        {
            List<tbl_special_offers> offers = new List<tbl_special_offers>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"SELECT * FROM vendores.tbl_special_offers 
                                WHERE (@validFrom IS NULL OR validFrom >= @validFrom) 
                                AND (@validTo IS NULL OR validTo <= @validTo)";
                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@validFrom", validFrom.HasValue ? (object)validFrom.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@validTo", validTo.HasValue ? (object)validTo.Value : DBNull.Value);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    offers.Add(new tbl_special_offers()
                    {
                        so_id = Convert.ToInt32(rd["so_id"]),
                        restaurant_id = Convert.ToInt32(rd["restaurant_id"]),
                        offer_title = rd["offer_title"].ToString(),
                        offer_desc = rd["offer_desc"].ToString(),
                        percentage_disc = Convert.ToInt32(rd["percentage_disc"]),
                        validFrom = Convert.ToDateTime(rd["validFrom"]),
                        validTo = Convert.ToDateTime(rd["validTo"]),
                        is_Active = Convert.ToBoolean(rd["is_Active"]),
                        menu_id = Convert.ToInt32(rd["menu_id"]),
                        ImagePath = rd["image_path"].ToString()
                    });
                }
                conn.Close();
            }
            return offers;
        }


        public List<tbl_vendor_feedback> GetAllFeedback()
        {
            var list = new List<tbl_vendor_feedback>();
            using (SqlConnection con = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("sp_GetVendorFeedback", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new tbl_vendor_feedback
                        {
                            vendore_feedback_id = Convert.ToInt32(rdr["vendore_feedback_id"]),
                            restaurant_id = Convert.ToInt32(rdr["restaurant_id"]),
                            rating = Convert.ToDecimal(rdr["rating"]),
                            feedback_description = rdr["feedback_description"].ToString(),
                            createdAt = Convert.ToDateTime(rdr["createdAt"])
                        });
                    }
                }
            }
            return list;
        }

        public void InsertFeedback(tbl_vendor_feedback feedback)
        {
            if (feedback.rating < 1 || feedback.rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            using (SqlConnection con = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("sp_InsertVendorFeedback", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@restaurant_id", feedback.restaurant_id);
                cmd.Parameters.AddWithValue("@rating", feedback.rating);
                cmd.Parameters.AddWithValue("@feedback_description", feedback.feedback_description);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<PayoutsDetailsViewModel> GetWeeklySalesByMonth(int year, int month, int resId)
        {
            var result = new List<PayoutsDetailsViewModel>();

            using (var conn = new SqlConnection(_connectionstring))
            using (var cmd = new SqlCommand("[Vendores].[GetWeeklySalesbyMonth]", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@res_id", resId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new PayoutsDetailsViewModel
                        {
                            WeekNumber = reader.GetInt32(0),
                            TotalAmount = reader.GetDecimal(1)
                        });
                    }
                }
            }
            return result;
        }

        public IEnumerable<PayOutViewModel> GetWeeklyPayouts(PayoutfilterModel filter)
        {
            var payouts = new List<PayOutViewModel>();

            using (var connection = new SqlConnection(_connectionstring))
            {
                connection.Open();

                var query = @"
                            WITH DateRange AS (
                                SELECT DATEADD(DAY, n, DATEFROMPARTS(@Year, @Month, 1)) AS Date
                                FROM (SELECT TOP (31) ROW_NUMBER() OVER (ORDER BY object_id) - 1 AS n FROM sys.objects) AS Numbers
                                WHERE DATEADD(DAY, n, DATEFROMPARTS(@Year, @Month, 1)) < DATEADD(MONTH, 1, DATEFROMPARTS(@Year, @Month, 1))
                            )
                            SELECT 
                                CONCAT(
                                    MIN(DAY(d.Date)), '–', 
                                    MAX(DAY(d.Date)), ' ', 
                                    DATENAME(MONTH, MIN(d.Date))
                                ) AS WeekRange,
                                ISNULL(SUM(o.grand_total), 0) AS OrderValue,
                                ISNULL(SUM(o.grand_total), 0) * 0.10 AS Commission,
                                ISNULL(SUM(o.grand_total), 0) * 0.18 AS GST,
                                ISNULL(SUM(o.grand_total), 0) - (ISNULL(SUM(o.grand_total), 0) * 0.10 + ISNULL(SUM(o.grand_total), 0) * 0.18) AS NetPayout,
                                CONVERT(VARCHAR, MAX(p.payment_date), 106) AS PaymentDate
                            FROM customers.tbl_orders o
                            JOIN DateRange d ON CAST(o.order_date AS DATE) = d.Date
                            LEFT JOIN customers.payments p ON o.order_id = p.order_id AND p.payment_status = 'paid'
                            WHERE o.resturant_id = @RestaurantId
                            GROUP BY DATEPART(WEEK, d.Date) - DATEPART(WEEK, DATEFROMPARTS(@Year, @Month, 1)) + 1
                            ORDER BY DATEPART(WEEK, d.Date) - DATEPART(WEEK, DATEFROMPARTS(@Year, @Month, 1)) + 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RestaurantId", filter.restaurant_id);
                    command.Parameters.AddWithValue("@Year", filter.Year);
                    command.Parameters.AddWithValue("@Month", filter.Month);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payouts.Add(new PayOutViewModel
                            {
                                WeekRange = reader["WeekRange"].ToString(),
                                OrderValue = reader["OrderValue"] != DBNull.Value ? Convert.ToDecimal(reader["OrderValue"]) : 0,
                                Commission = reader["Commission"] != DBNull.Value ? Convert.ToDecimal(reader["Commission"]) : 0,
                                GST = reader["GST"] != DBNull.Value ? Convert.ToDecimal(reader["GST"]) : 0,
                                NetPayout = reader["NetPayout"] != DBNull.Value ? Convert.ToDecimal(reader["NetPayout"]) : 0,
                                PaymentDate = reader["PaymentDate"] != DBNull.Value ? reader["PaymentDate"].ToString() : "N/A"
                            });
                        }
                    }
                }
            }

            return payouts;
        }

        public EarningSummaryViewModel GetEarningsSummary()
        {
            var summary = new EarningSummaryViewModel();

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                var query = @"SELECT SUM(oi.list_price * oi.quantity) 
                  FROM customers.tbl_orders o
                  JOIN customers.tbl_order_items oi ON o.order_id = oi.order_id";

                SqlCommand cmd = new SqlCommand(query, conn);
                var totalResult = cmd.ExecuteScalar();
                summary.TotalEarnings = totalResult != DBNull.Value ? Convert.ToDecimal(totalResult) : 0;

                // Calculate settled amount from payments
                var settledCommand = new SqlCommand(
                    @"SELECT SUM(Amount) 
                  FROM customers.payments 
                  WHERE payment_status = 'Complete'",
                    conn);

                var settledResult = settledCommand.ExecuteScalar();
                summary.SettledAmount = settledResult != DBNull.Value ? Convert.ToDecimal(settledResult) : 0;

                // Calculate pending amount
                summary.PendingAmount = summary.TotalEarnings - summary.SettledAmount;
            }
            return summary;
        }

        public IEnumerable<tbl_orders> GetOrders()
        {
            var orders = new List<tbl_orders>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();

                string query = @"SELECT order_id, order_date, grand_total FROM customers.tbl_orders";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new tbl_orders
                        {
                            order_id = Convert.ToInt32(reader["order_id"]),
                            order_date = Convert.ToDateTime(reader["order_date"]),
                            grand_total = Convert.ToDecimal(reader["grand_total"])
                        });
                    }
                }
                conn.Close();
            }
            return orders;
        }

        public IEnumerable<payments> GetPayments()
        {
            var payments = new List<payments>();

            using(SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                string query = @"SELECT payment_id, order_id, amount, payment_date, payment_status FROM customers.payments";
                SqlCommand cmd = new SqlCommand(query, conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new payments
                        {
                            payment_id = Convert.ToInt32(reader["payment_id"]),
                            order_id = Convert.ToInt32(reader["order_id"]),
                            amount = Convert.ToDecimal(reader["amount"]),
                            payment_date = Convert.ToDateTime(reader["payment_date"]),
                            payment_status = reader["payment_status"].ToString()
                        });
                    }
                }
                conn.Close();
            }
            return payments;
        }

        private void AddTableHeader(PdfPTable table, string text, iTextFont font)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = new BaseColor(0, 102, 204),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        public byte[] GenerateWeeklyPayoutsPdf(PayoutfilterModel filter)
        {
            var payouts = GetWeeklyPayouts(filter);
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(filter.Month ?? DateTime.Now.Month);

            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(doc, ms);

                doc.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLUE);
                doc.Add(new Paragraph($"Weekly Payouts Report - {monthName} {filter.Year}", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                });

                // Add table
                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 1.5f, 1.5f, 1f, 1f, 1.5f, 1.5f });

                // Add headers
                var headerFont = iTextFontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, iTextBaseColor.WHITE);
                AddPdfTableHeader(table, "Week", headerFont);
                AddPdfTableHeader(table, "Order Value", headerFont);
                AddPdfTableHeader(table, "Commission", headerFont);
                AddPdfTableHeader(table, "GST", headerFont);
                AddPdfTableHeader(table, "Net Payout", headerFont);
                AddPdfTableHeader(table, "Payment Date", headerFont);

                // Add data rows
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var payout in payouts)
                {
                    table.AddCell(new PdfPCell(new Phrase(payout.WeekRange, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase($"₹{payout.OrderValue:N2}", cellFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase($"₹{payout.Commission:N2}", cellFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase($"₹{payout.GST:N2}", cellFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase($"₹{payout.NetPayout:N2}", cellFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase(payout.PaymentDate, cellFont)));
                }

                doc.Add(table);
                doc.Close();

                return ms.ToArray();
            }
        }

        private void AddPdfTableHeader(PdfPTable table, string text, iTextFont font)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = new BaseColor(0, 102, 204),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        public byte[] GenerateWeeklyPayoutsExcel(PayoutfilterModel filter)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var payouts = GetWeeklyPayouts(filter);
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(filter.Month ?? DateTime.Now.Month);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add($"{monthName} {filter.Year}");

                // Add headers
                worksheet.Cells[1, 1].Value = "Week";
                worksheet.Cells[1, 2].Value = "Order Value";
                worksheet.Cells[1, 3].Value = "Commission";
                worksheet.Cells[1, 4].Value = "GST";
                worksheet.Cells[1, 5].Value = "Net Payout";
                worksheet.Cells[1, 6].Value = "Payment Date";

                // Format headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // Add data
                var row = 2;
                foreach (var payout in payouts)
                {
                    worksheet.Cells[row, 1].Value = payout.WeekRange;
                    worksheet.Cells[row, 2].Value = payout.OrderValue;
                    worksheet.Cells[row, 3].Value = payout.Commission;
                    worksheet.Cells[row, 4].Value = payout.GST;
                    worksheet.Cells[row, 5].Value = payout.NetPayout;
                    worksheet.Cells[row, 6].Value = payout.PaymentDate;

                    // Format currency cells
                    worksheet.Cells[row, 2].Style.Numberformat.Format = "\"₹\"#,##0.00";
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "\"₹\"#,##0.00";
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "\"₹\"#,##0.00";
                    worksheet.Cells[row, 5].Style.Numberformat.Format = "\"₹\"#,##0.00";

                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}
