using System;
using Thesis.Relinq.Attributes;

namespace Thesis.Relinq.Benchmarks.ThesisPOCOs
{
    [Table(Name = "orders")]
    public class Order
    {
        [Column(Name = "OrderID")]
        public short OrderID { get; set; }

        [Column(Name = "CustomerID")]
        public string CustomerID { get; set; }

        [Column(Name = "EmployeeID")]
        public short EmployeeID { get; set; }

        [Column(Name = "OrderDate")]
        public DateTime OrderDate { get; set; }

        [Column(Name = "RequiredDate")]
        public DateTime RequiredDate { get; set; }

        [Column(Name = "ShippedDate")]
        public DateTime ShippedDate { get; set; }

        [Column(Name = "ShipVia")]
        public short ShipVia { get; set; }

        [Column(Name = "Freight")]
        public float Freight { get; set; }

        [Column(Name = "ShipName")]
        public string ShipName { get; set; }

        [Column(Name = "ShipAddress")]
        public string ShipAddress { get; set; }

        [Column(Name = "ShipCity")]
        public string ShipCity { get; set; }

        [Column(Name = "ShipRegion")]
        public string ShipRegion { get; set; }

        [Column(Name = "ShipPostalCode")]
        public string ShipPostalCode { get; set; }

        [Column(Name = "ShipCountry")]
        public string ShipCountry { get; set; }
    }
}