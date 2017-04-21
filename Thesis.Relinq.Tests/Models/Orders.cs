using System;

namespace Thesis.Relinq.Tests.Models
{
    /*  OrderID        | smallint
        CustomerID     | character
        EmployeeID     | smallint
        OrderDate      | date
        RequiredDate   | date
        ShippedDate    | date
        ShipVia        | smallint
        Freight        | real
        ShipName       | character varying
        ShipAddress    | character varying
        ShipCity       | character varying
        ShipRegion     | character varying
        ShipPostalCode | character varying
        ShipCountry    | character varying  */

    public class Orders
    {
        public short OrderID { get; set; }
        public string CustomerID { get; set; }
        public short EmployeeID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public short ShipVia { get; set; }
        public float Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
    }
}