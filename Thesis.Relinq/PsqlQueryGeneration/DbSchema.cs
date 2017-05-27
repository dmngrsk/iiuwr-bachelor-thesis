using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Thesis.Relinq.PsqlQueryGeneration
{
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

        public string GetMatchingColumnName(string parsableName)
        {
            // We're using the fact that columns are always called following its tables.
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