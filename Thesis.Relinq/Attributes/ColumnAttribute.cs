using System;

namespace Thesis.Relinq.Attributes
{
    /// An attribute that classifies a property as a PostgreSQL column.
    public class ColumnAttribute : Attribute
    {
        /// If set, overrides the default PostgreSQL's table column with its value.
        public string Name { get; set; }
    }
}