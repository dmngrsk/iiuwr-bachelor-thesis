using NUnit.Framework;
using System.Linq;
using Thesis.Relinq.NpgsqlWrapper;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;

namespace Thesis.Relinq.Tests
{
    [TestFixture]
    public class MethodCallsTests : ThesisTestsBase
    {
        [Test]
        public void equals()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID.Equals(5)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID.Equals(5));
            
            string psqlCommand = "SELECT * FROM Employees WHERE (\"EmployeeID\" = 5);";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void take()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees LIMIT 5;";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.Take(5).ToArray();
            var actual2 = myQuery2.Take(5).ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void skip()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5;";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.Skip(5).ToArray();
            var actual2 = myQuery2.Skip(5).ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void take_and_skip()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5 LIMIT 3;";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.Skip(5).Take(3).ToArray();
            var actual2 = myQuery2.Take(3).Skip(5).ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void to_lower()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.ContactName.ToLower();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.ContactName.ToLower());
            
            string psqlCommand = "SELECT LOWER(\"ContactName\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void to_upper()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.ContactName.ToUpper();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.ContactName.ToUpper());
            
            string psqlCommand = "SELECT UPPER(\"ContactName\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void contains()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.ContactName.Contains("A")
                select c.ContactName;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.ContactName.Contains("A"))
                .Select(c => c.ContactName);
            
            string psqlCommand = "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE '%A%';";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void starts_with()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.ContactName.StartsWith("C")
                select c.ContactName;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.ContactName.StartsWith("C"))
                .Select(c => c.ContactName);
            
            string psqlCommand = "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE 'C%';";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void ends_with()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.ContactName.EndsWith("e")
                select c.ContactName;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.ContactName.EndsWith("e"))
                .Select(c => c.ContactName);
            
            string psqlCommand = "SELECT \"ContactName\" FROM Customers " +
                "WHERE \"ContactName\" LIKE '%e';";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void substring()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void length()
        {
            
        }


        [Test]
        public void trim()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.Trim(new char[] { 'A', 'B' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.Trim(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(both 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void trim_start()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.TrimStart(new char[] { 'A', 'B' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimStart(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(leading 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void trim_end()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.TrimEnd(new char[] { 'A', 'B' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimEnd(new char[] { 'A', 'B' }));
            
            string psqlCommand = "SELECT TRIM(trailing 'AB' from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void trim_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.Trim();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.Trim());
            
            string psqlCommand = "SELECT TRIM(both from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void trim_start_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.TrimStart();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimStart());
            
            string psqlCommand = "SELECT TRIM(leading from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void trim_end_whitespace()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.TrimEnd();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimEnd());
            
            string psqlCommand = "SELECT TRIM(trailing from \"CustomerID\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void concat()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void reverse()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void replace()
        {
            
        }
    }
}