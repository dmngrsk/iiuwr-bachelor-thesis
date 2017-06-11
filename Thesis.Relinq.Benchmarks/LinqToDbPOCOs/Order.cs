using System;
using LinqToDB.Mapping;

namespace Thesis.Relinq.Benchmarks.LinqToDbPOCOs
{
    [Table(Name = "orders")]
    public class Order
    {
        [PrimaryKey, Identity]
        public short OrderID { get; set; }

        [Column]
        public string CustomerID { get; set; }

        [Column]
        public short EmployeeID { get; set; }     

        [Column]
        public DateTime OrderDate { get; set; }   

        [Column]
        public DateTime RequiredDate { get; set; }

        [Column]
        public DateTime ShippedDate { get; set; }

        [Column]
        public short ShipVia { get; set; }       

        [Column]
        public float Freight { get; set; }       

        [Column]
        public string ShipName { get; set; }     

        [Column]
        public string ShipAddress { get; set; }  

        [Column]
        public string ShipCity { get; set; }     

        [Column]
        public string ShipRegion { get; set; }

        [Column]
        public string ShipPostalCode { get; set; }

        [Column]
        public string ShipCountry { get; set; }
    }
}