namespace Thesis.Relinq
{
    public class PostgresConnectionAdapter 
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public string ConnectionString
        {
            get
            {
                return string.Format(
                    "Server={0};Port={1};User Id={2};Password={3};Database={4};",
                    Server, Port, UserName, Password, Database);
            }
        }
    }
}