using Thesis.Relinq.Attributes;

namespace Thesis.Relinq.Tests.Models
{
    /*  CustomerID   | character
        CompanyName  | character varying
        ContactName  | character varying
        ContactTitle | character varying
        Address      | character varying
        City         | character varying
        Region       | character varying
        PostalCode   | character varying
        Country      | character varying
        Phone        | character varying
        Fax          | character varying  */

    [Table(Name = "customers")]
    public class Customer
    {
        [Column(Name = "CustomerID")]
        public string CustomerID { get; set; }

        [Column(Name = "CompanyName")]
        public string CompanyName { get; set; }

        [Column(Name = "ContactName")]
        public string ContactName { get; set; }

        [Column(Name = "ContactTitle")]
        public string ContactTitle { get; set; }

        [Column(Name = "Address")]
        public string Address { get; set; }

        [Column(Name = "City")]
        public string City { get; set; }

        [Column(Name = "Region")]
        public string Region { get; set; }

        [Column(Name = "PostalCode")]
        public string PostalCode { get; set; }

        [Column(Name = "Country")]
        public string Country { get; set; }

        [Column(Name = "Phone")]
        public string Phone { get; set; }

        [Column(Name = "Fax")]
        public string Fax { get; set; }
    }
}