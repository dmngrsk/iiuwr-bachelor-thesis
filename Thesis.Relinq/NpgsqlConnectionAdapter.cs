namespace Thesis.Relinq
{
    public class NpgsqlConnectionAdapter 
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public string ConnectionString
        {
            get
            {
                return string.Format(
                    "Server={0};Port={1};User Id={2};Password={3};Database={4};",
                    Server, Port, Username, Password, Database);
            }
        }
    }
}