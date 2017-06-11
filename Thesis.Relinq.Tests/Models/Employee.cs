using System;
using Thesis.Relinq.Attributes;

namespace Thesis.Relinq.Tests.Models
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

    [Table(Name = "employees")]
    public class Employee
    {
        [Column(Name = "EmployeeID")]
        public short EmployeeID { get; set; }

        [Column(Name = "LastName")]
        public string LastName { get; set; }

        [Column(Name = "FirstName")]
        public string FirstName { get; set; }

        [Column(Name = "Title")]
        public string Title { get; set; }

        [Column(Name = "TitleOfCourtesy")]
        public string TitleOfCourtesy { get; set; }

        [Column(Name = "BirthDate")]
        public DateTime BirthDate { get; set; }

        [Column(Name = "HireDate")]
        public DateTime HireDate { get; set; }

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

        [Column(Name = "HomePhone")]
        public string HomePhone { get; set; }

        [Column(Name = "Extension")]
        public string Extension { get; set; }

        [Column(Name = "Photo")]
        public byte[] Photo { get; set; }

        [Column(Name = "Notes")]
        public string Notes { get; set; }

        [Column(Name = "ReportsTo")]
        public short ReportsTo { get; set; }

        [Column(Name = "PhotoPath")]
        public string PhotoPath { get; set; }
    }
}