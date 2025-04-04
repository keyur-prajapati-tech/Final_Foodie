using Microsoft.Data.SqlClient;
using System.Data;

namespace Foodie.Repositories.For_DeliveryPartner
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, new()
    {
        private readonly string _connectionString;

        public GenericRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<T> GetAllData()
        {
            var item = new List<T>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string procedure = $"GetAll{typeof(T).Name}";
                SqlCommand cmd = new SqlCommand(procedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    T entiy = Activator.CreateInstance<T>();
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                        {
                            prop.SetValue(entiy, reader[prop.Name]);
                        }
                    }
                    item.Add(entiy);
                }
            }
            return (item);
        }

        public T GetDataById(int id)
        {
            T entity = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string procedure = $"Get{typeof(T).Name}ById"; // E.g., GetStudentById
                SqlCommand cmd = new SqlCommand(procedure, conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    entity = Activator.CreateInstance<T>();
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                        {
                            prop.SetValue(entity, reader[prop.Name]);
                        }
                    }
                }
            }
            return entity;
        }

        public void Add(T entity)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string procedure = $"Insert{typeof(T).Name}"; // Example: InsertStudent
                SqlCommand cmd = new SqlCommand(procedure, conn) { CommandType = CommandType.StoredProcedure };

                // Find the primary key dynamically
                var keyProperty = typeof(T).GetProperties()
                    .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)));

                foreach (var prop in typeof(T).GetProperties())
                {
                    // Skip primary key if found
                    if (keyProperty != null && prop.Name == keyProperty.Name)
                        continue;

                    // Add parameters for the procedure
                    cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public void Update(T entity)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string procedure = $"Update{typeof(T).Name}"; // E.g., UpdateStudent
                SqlCommand cmd = new SqlCommand(procedure, conn) { CommandType = CommandType.StoredProcedure };

                foreach (var prop in typeof(T).GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string procedure = $"Delete{typeof(T).Name}"; // E.g., DeleteStudent
                SqlCommand cmd = new SqlCommand(procedure, conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
