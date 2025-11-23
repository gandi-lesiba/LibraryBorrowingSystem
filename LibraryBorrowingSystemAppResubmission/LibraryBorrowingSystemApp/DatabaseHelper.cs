using System.Data;
using System.Data.SqlClient;

namespace LibraryManagementSystem
{
    public static class DataService
    {
        private static readonly string ConnectionString =
            @"Data Source=DUMIISANE;Initial Catalog=library;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;";

        public static DataTable RetrieveData(string query, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                adapter.Fill(dataTable);
            }

            return dataTable;
        }

        public static int ExecuteUpdate(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static object GetSingleValue(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }
    }
}