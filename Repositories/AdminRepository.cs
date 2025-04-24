using Foodie.Models;
using Foodie.ViewModels;
using Microsoft.Data.SqlClient;

namespace Foodie.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionstring;
        public AdminRepository (IConfiguration configuration)
        {
            _connectionstring = configuration.GetConnectionString("defaultconnection");
        }

        public bool AddAdmin(tbl_admin admin, byte[] Image)
        {
            using (SqlConnection conn = new SqlConnection(_connectionstring))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_Add_Admin", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Full_name", admin.Full_name);
                cmd.Parameters.AddWithValue("@Password", admin.Password);
                cmd.Parameters.AddWithValue("@Email", admin.Email);
                cmd.Parameters.AddWithValue("@Phone", admin.Phone);
                cmd.Parameters.AddWithValue("@role_id", admin.role_id);
                cmd.Parameters.AddWithValue("@Image", Image ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LastLogin", DateTime.Now);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                conn.Close();
                return rowsAffected > 0;
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
                        admin_id = Convert.ToInt32(reader["admin_id"]),
                        Full_name = reader["Full_name"].ToString(),
                        Password = reader["Password"].ToString(),
                        Email = reader["Email"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        Phone = reader["Phone"].ToString(),
                        LastLogin = Convert.ToDateTime(reader["LastLogin"]),
                        role_type = reader["role_type"].ToString(),
                        IMAGE = reader["Image"] as byte[]
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
                string query = "SELECT COUNT(*) FROM admins.tbl_admin WHERE Email = @Email AND Password = @Password";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@Password", Password);

                con.Open();

                int count = (int)cmd.ExecuteScalar(); // Returns the number of matching rows
                con.Close();

                return count > 0; // Login successful if count > 0
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
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        role_id = Convert.ToInt32(reader["role_id"]),
                        admin_id = (int)reader["admin_id"]
                        // EmployeeID = Convert.ToInt32(reader["EmployeeID"])
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
                        restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
                        //restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
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
                        customer_id = reader["customer_id"] != DBNull.Value ? (int)reader["customer_id"] : 0,
                        //restaurant_name = reader["restaurant_name"] != DBNull.Value ? reader["restaurant_name"].ToString() : string.Empty,
                        restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
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
                        restaurant_id = reader["restaurant_id"] != DBNull.Value ? (int)reader["restaurant_id"] : 0,
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
                        customer_complaint_id = (int)reader["partner_complaint_id"],
                        customer_name = reader["customer_name"].ToString(),
                        cmp_Descr = reader["rating"].ToString(),
                        cmp_Status = (int)reader["cmp_Status"],
                        Full_name = reader["Full_name"].ToString(),
                        restaurant_name = reader["restaurant_name"].ToString(),
                        ResolutionRemarks = reader["ResolutionRemarks"].ToString(),
                        createdAt = Convert.ToDateTime(reader["createdAt"]),
                        ResolvedAt = Convert.ToDateTime(reader["ResolvedAt"])
                    });
                }
                conn.Close();
            }
            return customer_complaints;
        }
        public void updateVencom(tbl_vendor_complaints tbl_Vendor_Complaints )
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
                    command.Parameters.AddWithValue("@resolvedAt",DateTime.Now);
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
    }
}
