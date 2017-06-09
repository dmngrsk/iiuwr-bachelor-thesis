using System;
using LinqToDB.Mapping;

namespace Thesis.Relinq.Benchmarks.LinqToDbPOCOs
{
    [Table(Name = "employees")]
    public class Employee
    {
        [PrimaryKey, Identity]
        public short EmployeeID { get; set; }

        [Column, NotNull]
        public string LastName { get; set; }
        
        [Column, NotNull]
        public string FirstName { get; set; }

        [Column]
        public string Title { get; set; }

        [Column]
        public string TitleOfCourtesy { get; set; }

        [Column]
        public DateTime BirthDate { get; set; }    

        [Column]
        public DateTime HireDate { get; set; }     

        [Column]
        public string Address { get; set; }        

        [Column]
        public string City { get; set; }           

        [Column]
        public string Region { get; set; }         

        [Column]
        public string PostalCode { get; set; }     

        [Column]
        public string Country { get; set; }        

        [Column]
        public string HomePhone { get; set; }      

        [Column]
        public string Extension { get; set; }      

        [Column]
        public byte[] Photo { get; set; }          

        [Column]
        public string Notes { get; set; }          

        [Column]
        public short ReportsTo { get; set; }

        [Column]
        public string PhotoPath { get; set; }
    }
}