using System;
using Npgsql;

namespace Thesis.Relinq
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                PostgresConnectionAdapter adapter = new PostgresConnectionAdapter
                {
                    Server = "localhost",
                    Port = 5432,
                    UserName = "dmngrsk",
                    Password = "qwerty",
                    Database = "northwind"
                };

                using (NpgsqlConnection conn = new NpgsqlConnection(adapter.ConnectionString))
                using (var cmd = new NpgsqlCommand()) 
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM customers";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] columns = new object[reader.FieldCount]; reader.GetValues(columns);
                            
                            foreach (var value in columns)
                                Console.WriteLine(value.ToString());

                            Console.WriteLine();
                        }
                    }

                    conn.Close();
                }
            }

            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }
        }
    }
}