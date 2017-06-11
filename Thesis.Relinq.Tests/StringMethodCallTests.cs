using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dapper;
using Thesis.Relinq.Tests.Helpers;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class StringMethodCallTests : TestClassBase
    {
        [Fact]
        public void to_lower()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.ContactName.ToLower();

            var myQuery2 = Context.Customers
                .Select(c => c.ContactName.ToLower());
            
            string psqlCommand = "SELECT LOWER(\"ContactName\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void to_upper()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.ContactName.ToUpper();

            var myQuery2 = Context.Customers
                .Select(c => c.ContactName.ToUpper());
            
            string psqlCommand = "SELECT UPPER(\"ContactName\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void contains()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                where c.ContactName.Contains("A")
                select c.ContactName;

            var myQuery2 = Context.Customers
                .Where(c => c.ContactName.Contains("A"))
                .Select(c => c.ContactName);
            
            string psqlCommand = 
                "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE '%A%';";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void starts_with()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                where c.ContactName.StartsWith("C")
                select c.ContactName;

            var myQuery2 = Context.Customers
                .Where(c => c.ContactName.StartsWith("C"))
                .Select(c => c.ContactName);
            
            string psqlCommand = 
                "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE 'C%';";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void ends_with()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                where c.ContactName.EndsWith("e")
                select c.ContactName;

            var myQuery2 = Context.Customers
                .Where(c => c.ContactName.EndsWith("e"))
                .Select(c => c.ContactName);
            
            string psqlCommand = 
                "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE '%e';";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void substring()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.ContactName.Substring(1);

            var myQuery2 = Context.Customers
                .Select(x => x.CustomerID.Substring(0, 2));

            var psqlCommand = "SELECT SUBSTRING(\"ContactName\" FROM 2) FROM Customers;";
            var psqlCommand2 = "SELECT SUBSTRING(\"CustomerID\" FROM 1 FOR 2) FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var expected2 = Connection.Query<string>(psqlCommand2);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected2, actual2);
        }

        [Fact]
        public void length()
        {
            // Arrange
            var myQuery =
                from c in Context.Customers
                select new
                {
                    Name = c.ContactName,
                    Length = c.ContactName.Length
                };

            var myQuery2 = Context.Customers
                .Select(c => new 
                               {
                                   Name = c.ContactName,
                                   Length = c.ContactName.Length
                               });

            var psqlCommand = "SELECT \"ContactName\", LENGTH(\"ContactName\") FROM Customers;";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.Trim(new char[] { 'A', 'B' });

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.Trim(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(both 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim_start()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.TrimStart(new char[] { 'A', 'B' });

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.TrimStart(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(leading 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim_end()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.TrimEnd(new char[] { 'A', 'B' });

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.TrimEnd(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(trailing 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.Trim();

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.Trim());
            
            string psqlCommand = "SELECT TRIM(both from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim_start_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.TrimStart();

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.TrimStart());
            
            string psqlCommand = "SELECT TRIM(leading from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void trim_end_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.TrimEnd();

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.TrimEnd());
            
            string psqlCommand = "SELECT TRIM(trailing from \"CustomerID\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void concat_on_strings()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select string.Concat(c.ContactName, " is from ", c.Country);

            var myQuery2 = Context.Customers
                .Select(c => string.Concat(c.ContactName, " is from ", c.Country));

            var psqlCommand = 
                "SELECT CONCAT(\"ContactName\", ' is from ', \"Country\") " +
                "FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void replace()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c.CustomerID.Replace('A', '0');

            var myQuery2 = Context.Customers
                .Select(c => c.CustomerID.Replace("A", "Hello"));

            var psqlCommand = "SELECT REPLACE(\"CustomerID\", 'A', '0') FROM Customers;";
            var psqlCommand2 = "SELECT REPLACE(\"CustomerID\", 'A', 'Hello') FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var expected2 = Connection.Query<string>(psqlCommand2);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected2, actual2);
        }
    }
}