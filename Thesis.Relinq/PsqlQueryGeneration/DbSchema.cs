using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Represents a schema of a database and allows to fetch the names of its tables and their columns.
    public class DbSchema
    {
        private DbConnection _connection;
        private List<string> _tables;
        private Dictionary<string, List<string>> _columns;

        private string _lastVisitedTable;
        private static string _alphanumericFilter = @"[^a-zA-Z0-9]";

        public DbSchema(DbConnection connection)
        {
            _connection = connection;
            _columns = new Dictionary<string, List<string>>();
        }

        /// Returns a table name in the database of the corresponding name provided in the argument.
        public string GetMatchingTableName(string parsableName)
        {
            if (_tables == null)
            {
                var tablesQuery = "SELECT tablename FROM pg_tables WHERE schemaname = 'public';";
                _tables = _connection.Query<string>(tablesQuery).AsList();
            }

            _lastVisitedTable = _tables.Find(x => NamesLowercasedAndAlphanumericMatch(x, parsableName));
            return _lastVisitedTable ?? throw new ArgumentException($"Table {parsableName} does not exist in database {_connection.Database}.");
        }

        /// Returns a column name in the database of the corresponding name provided in the argument.
        /// 
        /// Remarks: it is always following a GetMatchingTableName(string) call, so this method knows the table it needs to read a column name from.
        public string GetMatchingColumnName(string parsableName)
        {
            if (!_columns.ContainsKey(_lastVisitedTable))
            {
                var columnsQuery = $"SELECT column_name FROM information_schema.columns WHERE table_name='{_lastVisitedTable}';";
                _columns[_lastVisitedTable] = _connection.Query<string>(columnsQuery).AsList();
            }

            var match = _columns[_lastVisitedTable].Find(x => NamesLowercasedAndAlphanumericMatch(x, parsableName));
            return match ?? throw new ArgumentException($"Column {parsableName} does not exist in table {_lastVisitedTable}.");
        }

        private bool NamesLowercasedAndAlphanumericMatch(string parsable, string target)
        {
            return Regex.Replace(parsable, _alphanumericFilter, "").ToLower() == Regex.Replace(target, _alphanumericFilter, "").ToLower();
        }
    }
}