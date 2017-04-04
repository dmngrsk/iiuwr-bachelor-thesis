using NUnit.Framework;
using System.Linq;
using Thesis.Relinq.NpgsqlWrapper;
using Thesis.Relinq.UnitTests.Models;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class MethodCallsTests : ThesisTestsBase
    {
        [Test]
        public void equals()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Employees>(connection)
                where c.EmployeeID.Equals(5)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(c => c.EmployeeID.Equals(5));
            
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
        public void to_lower()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.ToLower();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.ToLower());
            
            string psqlCommand = "SELECT lower(\"CustomerID\") FROM Customers;";

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
            
            string psqlCommand = "SELECT upper(\"ContactName\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void contains()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void substring()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void length()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void starts_with()
        {
            
        }

        [Test, IgnoreAttribute("Feature not implemented yet")]
        public void ends_with()
        {
            
        }

        [Test]
        public void trim()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.CustomerID.Trim(new char[] { 'A' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.Trim(new char[] { 'A' }));
            
            string psqlCommand = "SELECT trim(both 'A' from \"CustomerID\") FROM Customers;";

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
                select c.CustomerID.TrimStart(new char[] { 'A' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimStart(new char[] { 'A' }));
            
            string psqlCommand = "SELECT trim(leading 'A' from \"CustomerID\") FROM Customers;";

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
                select c.CustomerID.TrimEnd(new char[] { 'A' });

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.CustomerID.TrimEnd(new char[] { 'A' }));
            
            string psqlCommand = "SELECT trim(trailing 'A' from \"CustomerID\") FROM Customers;";

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
            
            string psqlCommand = "SELECT trim(both from \"CustomerID\") FROM Customers;";

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
            
            string psqlCommand = "SELECT trim(leading from \"CustomerID\") FROM Customers;";

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
            
            string psqlCommand = "SELECT trim(trailing from \"CustomerID\") FROM Customers;";

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
    }
}
