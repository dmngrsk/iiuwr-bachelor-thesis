using System;

namespace Thesis.Relinq.Attributes
{
    /// An attribute that classifies a class as a PostgreSQL table.
    public class TableAttribute : Attribute
    {
        /// If set, overrides the default PostgreSQL's database table name with its value.
        public string Name { get; set; }
    }
}