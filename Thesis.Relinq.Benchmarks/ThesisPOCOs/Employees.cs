using System;

namespace Thesis.Relinq.Benchmarks.ThesisPOCOs
{
    /*  EmployeeID      | smallint
        LastName        | character varying
        FirstName       | character varying
        Title           | character varying
        TitleOfCourtesy | character varying
        BirthDate       | date
        HireDate        | date
        Address         | character varying
        City            | character varying
        Region          | character varying
        PostalCode      | character varying
        Country         | character varying
        HomePhone       | character varying
        Extension       | character varying
        Photo           | bytea
        Notes           | text
        ReportsTo       | smallint
        PhotoPath       | character varying  */

    public class Employees
    {
        public short EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public byte[] Photo { get; set; }
        public string Notes { get; set; }
        public short ReportsTo { get; set; }
        public string PhotoPath { get; set; }
    }
}