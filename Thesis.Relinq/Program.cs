using System;
using Npgsql;
using System.Linq;

namespace Thesis.Relinq
{
    public class Customers
    {
        public string MyProperty { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                PsqlConnectionAdapter adapter = new PsqlConnectionAdapter
                {
                    Server = "localhost",
                    Port = 5432,
                    Username = "dmngrsk",
                    Password = "qwerty",
                    Database = "northwind"
                };

                using (NpgsqlConnection conn = new NpgsqlConnection(adapter.ConnectionString))
                {
                    var foo = from c in PsqlQueryFactory.Queryable<Customers>(conn) select c;
                    var res = foo.ToList(); 
                    Console.WriteLine(res.Count);
                }
                /*using (var cmd = new NpgsqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM employees";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach (var foo in reader.GetColumnSchema())
                                Console.WriteLine(foo.ColumnName);

                            object[] columns = new object[reader.FieldCount]; reader.GetValues(columns);
                            
                            //foreach (var value in columns)
                            //    Console.WriteLine(string.Format("{0} {1}", value.GetType().ToString(), value));

                            Console.WriteLine();
                        }
                    }

                    conn.Close();
                }*/
            }

            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }
        }
    }
}