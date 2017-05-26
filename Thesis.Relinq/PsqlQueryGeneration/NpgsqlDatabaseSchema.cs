using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Npgsql;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NpgsqlDatabaseSchema
    {
        private string[] _tables;
        private Dictionary<string, string[]> _columns;
        private string _lastVisitedTable;
        private NpgsqlConnection _connection;

        public NpgsqlDatabaseSchema(NpgsqlConnection connection)
        {
            _connection = connection;
            _columns = new Dictionary<string, string[]>();
        }

        public string GetTableName(string parsableName)
        {
            if (_tables == null)
            {
                var tablesQuery = "SELECT tablename FROM pg_tables WHERE schemaname = 'public';";
                using (var command = new NpgsqlCommand(tablesQuery, _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var names = new List<string>();

                        while (reader.Read())
                        {
                            names.Add((string)reader.GetValue(0));
                        }

                        _tables = names.ToArray();
                    }
                }
            }

            foreach (var name in _tables)
            {
                if (MatchCase(name, parsableName)) 
                {
                    _lastVisitedTable = name;
                    return name;
                }
            }

            throw new ArgumentException($"Relation {parsableName} does not exist.");
        }

        public string GetColumnName(string parsableName)
        {
            // We're using the fact that columns are always called following its tables
            if (!_columns.ContainsKey(_lastVisitedTable))
            {
                var columnsQuery = $"SELECT * FROM {_lastVisitedTable} WHERE false;";
                using (var command = new NpgsqlCommand(columnsQuery, _connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var columnSchema = reader.GetColumnSchema();
                        var names = new List<string>();

                        foreach (var column in columnSchema)
                        {
                            names.Add(column.ColumnName);
                        }

                        _columns[_lastVisitedTable] = names.ToArray();
                    }
                }
            }

            foreach (var name in _columns[_lastVisitedTable])
            {
                if (MatchCase(name, parsableName)) 
                {
                    return name;
                }
            }
                
            throw new ArgumentException($"Row {parsableName} does not exist in relation {_lastVisitedTable}.");
        }

        private string _filter = @"[^a-zA-Z0-9]";
        
        private bool MatchCase(string parsable, string target)
        {
            return Regex.Replace(parsable, _filter, "").ToLower() == Regex.Replace(target, _filter, "").ToLower();
        }
    }
}