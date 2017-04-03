using Npgsql;

namespace Thesis.Relinq.NpgsqlWrapper
{
    public class NpgsqlConnectionAdapter 
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ConnectionString
        {
            get
            {
                return $"Server={Server}; Port={Port}; Database={Database}; " +
                       $"Username={Username}; Password={Password};";
            }
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}