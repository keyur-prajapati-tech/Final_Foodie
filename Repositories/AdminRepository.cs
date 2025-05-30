using System.Data;
using System.Numerics;
using Foodie.Models;
using Foodie.Models.Admin;
using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Foodie.ViewModels;
using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;

namespace Foodie.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionstring;
        public AdminRepository(IConfiguration configuration)
        {
            _connectionstring = configuration.GetConnectionString("defaultconnection");
        }

        public bool AddAdmin(tbl_admin admin, byte[] Image)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand("admins.sp_Add_Admin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Full_name", admin.Full_name);
                    cmd.Parameters.AddWithValue("@Password", admin.Password);
                    cmd.Parameters.AddWithValue("@Email", admin.Email);
                    cmd.Parameters.AddWithValue("@Phone", admin.Phone);
                    cmd.Parameters.AddWithValue("@role_id", admin.role_id);
                    cmd.Parameters.AddWithValue("@LastLogin", DateTime.Now);

                    // Handle the Image parameter
                    SqlParameter imageParam = new SqlParameter("@Image", SqlDbType.VarBinary);
                    if (Image != null)
                        imageParam.Value = Image;
                    else
                        imageParam.Value = DBNull.Value;
                    cmd.Parameters.Add(imageParam);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<tbl_admin> GetAll()
        {
            List<tbl_admin> admins = new List<tbl_admin>();
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {

                SqlCommand cmd = new SqlCommand("admins.sp_Sel_Admin", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    admins.Add(new tbl_admin
                    {
                        admin_id = reader["admin_id"] != DBNull.Value ? Convert.ToInt32(reader["admin_id"]) : 0,
                        Full_name = reader["Full_name"] != DBNull.Value ? reader["Full_name"].ToString() : string.Empty,
                        Password = reader["Password"] != DBNull.Value ? reader["Password"].ToString() : string.Empty,
                        Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                        CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.MinValue,
                        Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : string.Empty,
                        LastLogin = reader["LastLogin"] != DBNull.Value ? Convert.ToDateTime(reader["LastLogin"]) : DateTime.MinValue, // nullable
                        role_type = reader["role_type"] != DBNull.Value ? reader["role_type"].ToString() : string.Empty,
                        IMAGE = reader["Image"] != DBNull.Value ? (byte[])reader["Image"] : null

                    });
                }
                conn.Close();
            }
            return admins;
        }

        public IEnumerable<tbl_roles> GetRole()
        {
            var Role = new List<tbl_roles>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_Sel_Role", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Role.Add(new tbl_roles
                    {
                        role_id = (int)reader["role_id"],
                        role_type = reader["role_type"].ToString()
                    });
                }
                conn.Close();
            }
            return Role;
        }

        public bool Login(string Email, string Password)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT admin_id FROM admins.tbl_admin WHERE Email = @Email AND Password = @Password";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@Password", Password);

                con.Open();
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    int adminId = Convert.ToInt32(result);

                    // Update LastLogin timestamp
                    string updateQuery = "UPDATE admins.tbl_admin SET LastLogin = @Now WHERE admin_id = @AdminId";
                    SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                    updateCmd.Parameters.AddWithValue("@Now", DateTime.Now);
                    updateCmd.Parameters.AddWithValue("@AdminId", adminId);
                    updateCmd.ExecuteNonQuery();

                    con.Close();
                    return true;
                }

                con.Close();
                return false;
            }
        }

        public tbl_admin getSessionData(string Email)
        {
            tbl_admin admin = null;

            string query = "SELECT * FROM admins.tbl_admin WHERE Email = @Email";

            using (SqlConnection connection = new SqlConnection(_connectionstring))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", Email);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); // Assuming email is unique, we can use Read() to get the first (and only) record
                    admin = new tbl_admin
                    {
                        Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                        Password = reader["Password"] != DBNull.Value ? reader["Password"].ToString() : null,
                        role_id = reader["role_id"] != DBNull.Value ? Convert.ToInt32(reader["role_id"]) : 0, // default 0 or handle as needed
                        IMAGE = reader["Image"] != DBNull.Value ? (byte[])reader["Image"] : null,
                        admin_id = reader["admin_id"] != DBNull.Value ? Convert.ToInt32(reader["admin_id"]) : 0 // or nullable int
                    };
                }

                connection.Close();
            }

            return admin; // Returns null if no employee is found
        }

        public IEnumerable<tbl_vendor_feedback> GetAllVendorFeedback()
        {
            var vendor_feedback = new List<tbl_vendor_feedback>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_vendor_feedback", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vendor_feedback.Add(new tbl_vendor_feedback
                    {
                        vendore_feedback_id = reader["vendore_feedback_id"] != DBNull.Value ? (int)reader["vendore_feedback_id"] : 0,
                        //restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        rating = reader["rating"] != DBNull.Value ? Convert.ToDecimal(reader["rating"]) : 0m,
                        feedback_description = reader["feedback_description"] != DBNull.Value ? reader["feedback_description"].ToString() : string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue

                    });
                }
                conn.Close();
            }
            return vendor_feedback;
        }
        public IEnumerable<tbl_customer_feedback> GetAllCustomerFeedback()
        {
            var customer_feedback = new List<tbl_customer_feedback>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_customer_feedback", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    customer_feedback.Add(new tbl_customer_feedback
                    {
                        cust_feedback_id = reader["cust_feedback_id"] != DBNull.Value ? (int)reader["cust_feedback_id"] : 0,
                        //customer_id = reader["customer_id"] != DBNull.Value ? (int)reader["customer_id"] : 0,
                        restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        customer_name = reader["customer_name"] != DBNull.Value ? reader["customer_name"].ToString() : string.Empty,
                        //restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        rating = reader["rating"] != DBNull.Value ? Convert.ToDecimal(reader["rating"]) : 0m,
                        feedback_description = reader["feedback_description"] != DBNull.Value ? reader["feedback_description"].ToString() : string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue
                    });
                }
                conn.Close();
            }
            return customer_feedback;
        }
        public IEnumerable<tbl_delivery_feedback> GetAllDeliveryFeedback()
        {
            var delivery_feedback = new List<tbl_delivery_feedback>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_delivery_feedback", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    delivery_feedback.Add(new tbl_delivery_feedback
                    {
                        delivery_feedback_id = reader["delivery_feedback_id"] != DBNull.Value ? (int)reader["delivery_feedback_id"] : 0,
                        partner_id = reader["partner_id"] != DBNull.Value ? (int)reader["partner_id"] : 0,
                        //Fullname = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : string.Empty,
                        rating = reader["rating"] != DBNull.Value ? Convert.ToDecimal(reader["rating"]) : 0m,
                        feedback_description = reader["feedback_description"] != DBNull.Value ? reader["feedback_description"].ToString() : string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue

                    });
                }
                conn.Close();
            }
            return delivery_feedback;
        }

        public IEnumerable<tbl_vendor_complaints> GetAllVendorComplaints()
        {
            var vendor_complaints = new List<tbl_vendor_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_vendor_complaints", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vendor_complaints.Add(new tbl_vendor_complaints
                    {
                        vendor_complaint_id = reader["vendor_complaint_id"] != DBNull.Value ? (int)reader["vendor_complaint_id"] : 0,
                        //restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        cmp_Descr = reader["cmp_Descr"] != DBNull.Value ? reader["cmp_Descr"].ToString() : string.Empty,
                        cmp_Status = reader["cmp_Status"] != DBNull.Value ? Convert.ToBoolean(reader["cmp_Status"]) : false,
                        //admin_id = reader["admin_id"] != DBNull.Value ? (int)reader["admin_id"] : 0,
                        Full_name = reader["Full_name"] != DBNull.Value ? reader["Full_name"].ToString() : string.Empty,
                        ResolutionRemarks = reader["ResolutionRemarks"] != DBNull.Value ? reader["ResolutionRemarks"].ToString() : string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue,
                        ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : DateTime.MinValue
                    });
                }
                conn.Close();
            }
            return vendor_complaints;
        }

        public IEnumerable<tbl_partner_complaints> GetAllDeliveryComplaints()
        {
            var Delivery_complaints = new List<tbl_partner_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_delivery_complaints", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Delivery_complaints.Add(new tbl_partner_complaints
                    {
                        partner_complaint_id = reader["partner_complaint_id"] != DBNull.Value ? (int)reader["partner_complaint_id"] : 0,
                        partner_id = reader["partner_id"] != DBNull.Value ? (int)reader["partner_id"] : 0,
                        cmp_Descr = reader["cmp_Descr"] != DBNull.Value ? reader["cmp_Descr"].ToString() : string.Empty,
                        cmp_Status = reader["cmp_Status"] != DBNull.Value ? Convert.ToBoolean(reader["cmp_Status"]) : false,
                        admin_id = reader["admin_id"] != DBNull.Value ? (int)reader["admin_id"] : 0,
                        ResolutionRemarks = reader["ResolutionRemarks"] != DBNull.Value ? reader["ResolutionRemarks"].ToString() : string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue,
                        ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : DateTime.MinValue
                    });
                }
                conn.Close();
            }
            return Delivery_complaints;
        }

        public IEnumerable<tbl_customer_complaints> GetAllCustomerComplaints()
        {
            var customer_complaints = new List<tbl_customer_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_customer_complaints", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    customer_complaints.Add(new tbl_customer_complaints
                    {
                        customer_complaint_id = reader["partner_complaint_id"] is DBNull ? 0 : Convert.ToInt32(reader["partner_complaint_id"]),
                        customer_name = reader["customer_name"] is DBNull ? null : reader["customer_name"].ToString(),
                        cmp_Descr = reader["rating"] is DBNull ? null : reader["rating"].ToString(),
                        cmp_Status = reader["cmp_Status"] is DBNull ? 0 : Convert.ToInt32(reader["cmp_Status"]),
                        Full_name = reader["Full_name"] is DBNull ? null : reader["Full_name"].ToString(),
                        restaurant_name = reader["restaurant_name"] is DBNull ? null : reader["restaurant_name"].ToString(),
                        ResolutionRemarks = reader["ResolutionRemarks"] is DBNull ? null : reader["ResolutionRemarks"].ToString(),
                        createdAt = reader["createdAt"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(reader["createdAt"]),
                        ResolvedAt = reader["ResolvedAt"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(reader["ResolvedAt"])

                    });
                }
                conn.Close();
            }
            return customer_complaints;
        }
        public void updateVencom(tbl_vendor_complaints tbl_Vendor_Complaints)
        {
            using (var conn = new SqlConnection(_connectionstring))
            {
                //conn.Open();

                using (var command = new SqlCommand("admins.sp_edit_vendor_complaints", conn))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@vendor_com_id", tbl_Vendor_Complaints.vendor_complaint_id);
                    command.Parameters.AddWithValue("@cmp_status", 1);
                    command.Parameters.AddWithValue("@admin_id", tbl_Vendor_Complaints.admin_id);
                    command.Parameters.AddWithValue("@resolutionRemarks", tbl_Vendor_Complaints.ResolutionRemarks);
                    command.Parameters.AddWithValue("@resolvedAt", DateTime.Now);
                    // Execute the stored procedure
                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void updateDelcom(tbl_partner_complaints tbl_Partner_Complaints)
        {
            using (var conn = new SqlConnection(_connectionstring))
            {
                //conn.Open();

                using (var command = new SqlCommand("admins.sp_edit_partner_complaints", conn))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@partner_com_id", tbl_Partner_Complaints.partner_complaint_id);
                    command.Parameters.AddWithValue("@cmp_status", true);
                    command.Parameters.AddWithValue("@admin_id", tbl_Partner_Complaints.admin_id);
                    command.Parameters.AddWithValue("@resolutionRemarks", tbl_Partner_Complaints.ResolutionRemarks);
                    command.Parameters.AddWithValue("@resolvedAt", DateTime.Now);
                    // Execute the stored procedure
                    conn.Open();
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public IEnumerable<tbl_restaurant> GetAllRestaurant()
        {
            var restaurants = new List<tbl_restaurant>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_Sel_Restaurant", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    restaurants.Add(new tbl_restaurant
                    {
                        restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        restaurant_contact = reader["restaurant_contact"] != DBNull.Value ? reader["restaurant_contact"].ToString() : string.Empty,
                        restaurant_email = reader["restaurant_email"] != DBNull.Value ? reader["restaurant_email"].ToString() : string.Empty,
                        restaurant_street = reader["restaurant_street"] != DBNull.Value ? reader["restaurant_street"].ToString() : string.Empty,
                        restaurant_pincode = reader["restaurant_pincode"] != DBNull.Value ? reader["restaurant_pincode"].ToString() : string.Empty,
                        restaurant_isApprov = reader["restaurant_isApprov"] != DBNull.Value && Convert.ToBoolean(reader["restaurant_isApprov"]),
                        restaurant_isOnline = reader["restaurant_isOnline"] != DBNull.Value && Convert.ToBoolean(reader["restaurant_isOnline"]),
                        est_id = reader["est_id"] != DBNull.Value ? (int)reader["est_id"] : 0,
                        owner_name = reader["owner_name"] != DBNull.Value ? reader["owner_name"].ToString() : string.Empty

                    });
                }
                conn.Close();
            }
            return restaurants;
        }

        public IEnumerable<tbl_customer> GetAllCustomer()
        {
            var customers = new List<tbl_customer>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_Sel_Customer", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    customers.Add(new tbl_customer
                    {
                        customer_id = reader["customer_id"] != DBNull.Value ? (int)reader["customer_id"] : 0,
                        customer_name = reader["customer_name"] != DBNull.Value ? reader["customer_name"].ToString() : string.Empty,
                        email = reader["email"] != DBNull.Value ? reader["email"].ToString() : string.Empty,
                        phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : string.Empty,
                        Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : string.Empty,
                        profilepic = reader["profilepic"] != DBNull.Value ? (byte[])reader["profilepic"] : null,
                        DOB = reader["DOB"] != DBNull.Value ? Convert.ToDateTime(reader["DOB"]) : DateTime.MinValue,
                        created_at = reader["created_at"] != DBNull.Value ? Convert.ToDateTime(reader["created_at"]) : DateTime.MinValue,
                        updated_at = reader["updated_at"] != DBNull.Value ? Convert.ToDateTime(reader["updated_at"]) : DateTime.MinValue,

                    });
                }
                conn.Close();
            }
            return customers;
        }

        public int GetMonthlyCustomerCount()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_customers", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetMonthlyRestaurantCount()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Restaurant", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetCancelledOrders()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_cancle_orders", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetPendingOrders()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_pending_orders", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetAcceptedOrders()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_accepted_orders", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetDeliveredOrders()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Deliverd_orders", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetActiveRestaurants()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Active_Restaurant", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetInactiveRestaurants()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_InActive_Restaurant", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetOpenRestaurants()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Open_Restaurant", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public int GetClosedRestaurants()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Close_Restaurant", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                con.Close();

                return count;
            }
        }

        public decimal GetMonthlySales()
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_count_Total_Monthly_Sale", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                con.Open();

                object result = cmd.ExecuteScalar();
                con.Close();

                if (result == DBNull.Value || result == null)
                {
                    return 0m; // or handle appropriately
                }

                return Convert.ToDecimal(result);
            }
        }

        public List<OrderViewModel> GetAllOrders(string status)
        {
            var orders = new List<OrderViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                string query = @"
            select co.order_id , customer_name,order_status,restaurant_name,order_date,deliver_dateTime ,coupone_id , menu_name, discount  from 
            customers.tbl_orders co
            inner join customers.tbl_order_items coi
            on co.order_id = coi.order_id
            inner join customers.tbl_customer cc
            on co.customer_id = cc.customer_id
            inner join vendores.tbl_restaurant vr 
            on co.resturant_id = vr.restaurant_id
            inner join vendores.tbl_menu_items vmi
            on coi.menu_id = vmi.menu_id";

                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    query += " AND co.order_status = @status";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(status) && status != ".0All")
                {
                    cmd.Parameters.AddWithValue("@status", status);
                }

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new OrderViewModel
                    {
                        OrderId = reader["order_id"] != DBNull.Value ? Convert.ToInt32(reader["order_id"]) : 0,
                        order_status = reader["order_status"]?.ToString(),
                        RestaurantName = reader["restaurant_name"]?.ToString(),
                        OrderDate = reader["order_date"] != DBNull.Value ? Convert.ToDateTime(reader["order_date"]) : DateTime.MinValue,
                        DeliveryTime = reader["deliver_dateTime"] != DBNull.Value ? Convert.ToDateTime(reader["deliver_dateTime"]) : (DateTime?)null,
                        customer_name = reader["customer_name"]?.ToString(),
                        coupone_id = reader["coupone_id"] != DBNull.Value ? Convert.ToInt32(reader["coupone_id"]) : 0,
                        discount = reader["discount"] != DBNull.Value ? Convert.ToDecimal(reader["discount"]) : 0m,
                        Menu = reader["menu_name"] != DBNull.Value ? reader["menu_name"].ToString() : null,
                    });
                }
            }

            return orders;
        }

        public tbl_admin GetAdminById(int adminId)
        {
            var profile = new tbl_admin();

            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "SELECT Full_name, Email, Phone, IMAGE FROM admins.tbl_admin WHERE admin_id = @admin_id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@admin_id", adminId);
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            profile.Full_name = reader["Full_name"] != DBNull.Value ? reader["Full_name"].ToString() : string.Empty;
                            profile.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty;
                            profile.Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : string.Empty;

                            if (reader["IMAGE"] != DBNull.Value)
                            {
                                profile.IMAGE = (byte[])reader["IMAGE"];
                            }
                        }
                    }
                }
            }
            return profile;
        }

        public async void UpdateAdmin(tbl_admin admin)
        {
            using (SqlConnection con = new SqlConnection(_connectionstring))
            {
                string query = "UPDATE admins.tbl_admin SET Full_name=@Full_name, Email=@Email, Phone=@Phone, IMAGE=@IMAGE WHERE admin_id=@admin_id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Full_name", admin.Full_name);
                    cmd.Parameters.AddWithValue("@Email", admin.Email);
                    cmd.Parameters.AddWithValue("@Phone", admin.Phone);
                    cmd.Parameters.AddWithValue("@IMAGE", (object)admin.IMAGE ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@admin_id", admin.admin_id);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public List<tbl_cust_vendor_complaints> GetcustVendorComplaints()
        {
            var complaints = new List<tbl_cust_vendor_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_customer_ven_complaints", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    complaints.Add(new tbl_cust_vendor_complaints
                    {
                        vendor_complaint_id = reader["vendor_complaint_id"] != DBNull.Value ? (int)reader["vendor_complaint_id"] : 0,
                        restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        customer_id = reader["customer_id"] != DBNull.Value ? (int)reader["customer_id"] : 0,
                        cmp_Descr = reader["cmp_Descr"]?.ToString() ?? string.Empty,
                        cmp_Status = reader["cmp_Status"] != DBNull.Value && (bool)reader["cmp_Status"],
                        ResolutionRemarks = reader["ResolutionRemarks"]?.ToString() ?? string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue,
                        ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : DateTime.MinValue
                    });
                }
                conn.Close();
            }

            return complaints;
        }

        public List<tbl_cust_partner_complaints> GetcustPartnerComplaints()
        {
            var complaints = new List<tbl_cust_partner_complaints>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_sel_customer_par_complaints", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    complaints.Add(new tbl_cust_partner_complaints
                    {
                        partner_complaint_id = reader["partner_complaint_id"] != DBNull.Value ? (int)reader["partner_complaint_id"] : 0,
                        partner_id = reader["partner_id"] != DBNull.Value ? (int)reader["partner_id"] : 0,
                        customer_id = reader["customer_id"] != DBNull.Value ? (int)reader["customer_id"] : 0,
                        cmp_Descr = reader["cmp_Descr"]?.ToString() ?? string.Empty,
                        cmp_Status = reader["cmp_Status"] != DBNull.Value && (bool)reader["cmp_Status"],
                        ResolutionRemarks = reader["ResolutionRemarks"]?.ToString() ?? string.Empty,
                        createdAt = reader["createdAt"] != DBNull.Value ? Convert.ToDateTime(reader["createdAt"]) : DateTime.MinValue,
                        ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : DateTime.MinValue
                    });
                }
                conn.Close();
            }

            return complaints;
        }

        public void UpdateApprovalStatus(int restaurantId, bool isApproved)
        {
            using (var con = new SqlConnection(_connectionstring))
            {
                var cmd = new SqlCommand("vendores.sp_UpdateVendorApprovalStatus", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@restaurant_id", restaurantId);
                cmd.Parameters.AddWithValue("@isApproved", isApproved);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<VendoreViewModel> GetVendorsForApproval()
        {
            var vendors = new List<VendoreViewModel>();
            using (var con = new SqlConnection(_connectionstring))
            {
                var cmd = new SqlCommand("vendores.sp_GetVendorpendingDetailsForApproval", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vendors.Add(new VendoreViewModel
                    {
                        RestaurantId = reader["restaurant_id"] != DBNull.Value ? Convert.ToInt32(reader["restaurant_id"]) : 0,
                        RestaurantName = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : null,
                        RestaurantEmail = reader["restaurant_email"] != DBNull.Value ? reader["restaurant_email"].ToString() : null,
                        RestaurantContact = reader["restaurant_contact"] != DBNull.Value ? reader["restaurant_contact"].ToString() : null,
                        RestaurantStreet = reader["restaurant_street"] != DBNull.Value ? reader["restaurant_street"].ToString() : null,
                        OwnerName = reader["owner_name"] != DBNull.Value ? reader["owner_name"].ToString() : null,
                        OwnerContact = reader["owner_contact"] != DBNull.Value ? reader["owner_contact"].ToString() : null,
                        BankName = reader["bank_name"] != DBNull.Value ? reader["bank_name"].ToString() : null,
                        IFSCCode = reader["IFSC_code"] != DBNull.Value ? reader["IFSC_code"].ToString() : null,
                        ACCNo = reader["ACC_No"] != DBNull.Value ? reader["ACC_No"].ToString() : null,
                        ACCType = reader["ACC_Type"] != DBNull.Value ? reader["ACC_Type"].ToString() : null,
                        OwnerEmail = reader["owner_email"] != DBNull.Value ? reader["owner_email"].ToString() : null,
                        GstNumber = reader["gst_number"] != DBNull.Value ? reader["gst_number"].ToString() : null,
                        PanHolderName = reader["pan_holder_name"] != DBNull.Value ? reader["pan_holder_name"].ToString() : null,
                        PanNumber = reader["pan_number"] != DBNull.Value ? reader["pan_number"].ToString() : null,
                        FssaiCerti = reader["fssai_certi"] != DBNull.Value ? reader["fssai_certi"].ToString() : null,
                        ExpiryDate = reader["Ex_date"] != DBNull.Value ? Convert.ToDateTime(reader["Ex_date"]) : DateTime.MinValue,
                        RestaurantImage = reader["Restaurant_img"] != DBNull.Value ? reader["Restaurant_img"] as byte[] : null,
                        FssaiIsVerified = reader["fssai_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["fssai_isVerify"]) : false,
                        PanIsVerified = reader["pan_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["pan_isVerify"]) : false,
                        GstIsVerified = reader["gst_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["gst_isVerify"]) : false,
                        MenuImage = reader["Restaurant_menu_img"] != DBNull.Value ? reader["Restaurant_menu_img"] as byte[] : null,
                        PanImage = reader["pan_img"] != DBNull.Value ? reader["pan_img"] as byte[] : null,
                        FssaiImage = reader["fssai_img"] != DBNull.Value ? reader["fssai_img"] as byte[] : null

                    });
                }
            }
            return vendors;
        }
        public VendoreViewModel GetVendorById(int restaurantId)
        {
            VendoreViewModel vendor = null;

            using (var con = new SqlConnection(_connectionstring))
            {
                var cmd = new SqlCommand("vendores.sp_GetVendorDetailsForApprovalbyid", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@res_id", restaurantId);

                con.Open();
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    vendor = new VendoreViewModel
                    {
                        RestaurantId = reader["restaurant_id"] != DBNull.Value ? Convert.ToInt32(reader["restaurant_id"]) : 0,
                        RestaurantName = reader["restaurant_name"]?.ToString(),
                        RestaurantEmail = reader["restaurant_email"]?.ToString(),
                        RestaurantContact = reader["restaurant_contact"]?.ToString(),
                        RestaurantStreet = reader["restaurant_street"]?.ToString(),
                        RestaurantPincode = reader["restaurant_pincode"]?.ToString(),

                        OwnerName = reader["owner_name"]?.ToString(),
                        OwnerContact = reader["owner_contact"]?.ToString(),
                        OwnerEmail = reader["owner_email"]?.ToString(),

                        BankName = reader["bank_name"]?.ToString(),
                        IFSCCode = reader["IFSC_code"]?.ToString(),
                        ACCNo = reader["ACC_No"]?.ToString(),
                        ACCType = reader["ACC_Type"]?.ToString(),

                        PanNumber = reader["pan_number"]?.ToString(),
                        PanHolderName = reader["pan_holder_name"]?.ToString(),
                        PanIsVerified = reader["pan_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["pan_isVerify"]) : false,
                        PanImage = reader["pan_img"] != DBNull.Value ? (byte[])reader["pan_img"] : null,

                        GstNumber = reader["gst_number"]?.ToString(),
                        GstIsVerified = reader["gst_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["gst_isVerify"]) : false,

                        FssaiCerti = reader["fssai_certi"]?.ToString(),
                        ExpiryDate = reader["Ex_date"] != DBNull.Value ? Convert.ToDateTime(reader["Ex_date"]) : DateTime.MinValue,
                        FssaiImage = reader["fssai_img"] != DBNull.Value ? (byte[])reader["fssai_img"] : null,
                        FssaiIsVerified = reader["fssai_isVerify"] != DBNull.Value ? Convert.ToBoolean(reader["fssai_isVerify"]) : false,

                        RestaurantImage = reader["Restaurant_img"] != DBNull.Value ? (byte[])reader["Restaurant_img"] : null,
                        MenuImage = reader["Restaurant_menu_img"] != DBNull.Value ? (byte[])reader["Restaurant_menu_img"] : null
                    };
                }
            }

            return vendor;
        }

        public bool DeleteVendor(int id)
        {
            using (var con = new SqlConnection(_connectionstring))
            {
                var cmd = new SqlCommand("vendores.sp_DeleteVendorById", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@RestaurantId", id);
                con.Open();
                var rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public IEnumerable<tbl_restaurant> GetPendingRestaurant()
        {

            List<tbl_restaurant> alertList = new List<tbl_restaurant>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("vendores.sp_get_pending_restaurant", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    alertList.Add(new tbl_restaurant
                    {
                        restaurant_id = Convert.ToInt32(reader["restaurant_id"]),
                        restaurant_name = reader["restaurant_name"].ToString(),
                        restaurant_contact = reader["restaurant_contact"].ToString(),
                        restaurant_email = reader["restaurant_email"].ToString(),
                        owner_name = reader["owner_name"].ToString()
                    }); 
                }
            }

            return alertList;
        }




        public int AddCuisine(string cuisineName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("admins.sp_AddCuisine", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CuisineName", cuisineName);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public List<tbl_cuisine_master> GetAllCuisines()
        {
            var list = new List<tbl_cuisine_master>();
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("admins.sp_GetAllCuisines", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new tbl_cuisine_master
                        {
                            cuisine_id = (int)reader["cuisine_id"],
                            cuisine_name = reader["cuisine_name"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public void UpdateCuisine(int id, string name)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("admins.sp_UpdateCuisine", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CuisineId", id);
                cmd.Parameters.AddWithValue("@CuisineName", name);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteCuisine(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("admins.sp_DeleteCuisine", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CuisineId", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<OrderViewModel> GetFilteredOrders(DateTime? fromDate, DateTime? toDate, string orderStatus)
        {
            List<OrderViewModel> orders = new List<OrderViewModel>();
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                   SELECT o.order_id, o.order_date, o.order_status,
                   SUM(oi.list_price * oi.quantity - oi.discount) AS TotalAmount
                FROM customers.tbl_orders o
                JOIN customers.tbl_order_items oi ON o.order_id = oi.order_id
                WHERE (@startDate IS NULL OR o.order_date >= @startDate)
                  AND (@endDate IS NULL OR o.order_date <= @endDate)
                  AND (@status IS NULL OR o.order_status = @status)
                GROUP BY o.order_id, o.order_date, o.order_status", conn);

                cmd.Parameters.AddWithValue("@startDate", (object)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@endDate", (object)toDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", string.IsNullOrEmpty(orderStatus) ? DBNull.Value : (object)orderStatus);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    orders.Add(new OrderViewModel
                    {
                        OrderId = reader["order_id"] != DBNull.Value ? Convert.ToInt32(reader["order_id"]) : 0,
                       // UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                        OrderDate = reader["order_date"] != DBNull.Value ? Convert.ToDateTime(reader["order_date"]) : DateTime.MinValue,
                        order_status = reader["order_status"] != DBNull.Value ? reader["order_status"].ToString() : string.Empty,
                        //RazorpayOrderId = reader["RazorPayOrderId"] != DBNull.Value ? reader["RazorPayOrderId"].ToString() : string.Empty,
                        TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0,
                        //require_date = reader["require_date"] != DBNull.Value ? (DateTime?)reader["require_date"] : null,
                        // Uncomment and apply same logic for below fields if needed:
                        // OrderStatus = reader["order_status"] != DBNull.Value ? reader["order_status"].ToString() : string.Empty,
                        // CancelReason = reader["cancelreason"] != DBNull.Value ? reader["cancelreason"].ToString() : string.Empty,
                        //PaymentId = reader["PaymentId"] != DBNull.Value ? reader["PaymentId"].ToString() : string.Empty
                    });
                }
            }
            return orders;
        }
        public List<DashBoardViewModel> GetWeeklySales(int year, int month)
        {
            var sales = new List<DashBoardViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("[customers].[GetWeeklySales]", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Month", month);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new DashBoardViewModel
                        {
                            WeekNumber = reader.GetInt32(0),
                            TotalAmount = reader.GetDecimal(1)
                        });
                    }
                }
            }

            return sales;
        }

        public List<DashBoardViewModel> GetYearlySales(int year)
        {
            var sales = new List<DashBoardViewModel>();

            using (SqlConnection conn = new SqlConnection(_connectionstring))
            using (SqlCommand cmd = new SqlCommand("[customers].[GetYearlySales]", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", year);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new DashBoardViewModel
                        {
                            SalesMonth = reader.GetInt32(0),
                            TotalSales = reader.GetDecimal(1)
                        });
                    }
                }
            }

            return sales;
        }
        public bool UpdatePassword(string email, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand("[admins].[sp_UpdateAdminPassword]", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public bool ChangeResStatus(int resid, bool status)
        {
            using (var connection = new SqlConnection(_connectionstring))
            {
                connection.Open();
                var command = new SqlCommand(
                    "update vendores.tbl_restaurant set restaurant_isApprov = @status where restaurant_id =@resid ",
                    connection);
                command.Parameters.AddWithValue("@resid", resid);
                command.Parameters.AddWithValue("@status", status);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

    }
}