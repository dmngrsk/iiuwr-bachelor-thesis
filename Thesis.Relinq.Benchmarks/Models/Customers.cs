namespace Thesis.Relinq.Benchmarks.Models
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

    public class Customers
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }
}